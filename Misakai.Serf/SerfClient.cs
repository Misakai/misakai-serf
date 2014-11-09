using MsgPack;
using MsgPack.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Misakai.Serf
{
  
    /// <summary>
    /// Represents a RPC Client for Serf discovery service.
    /// </summary>
    public class SerfClient
    {
        private TcpClient Client;
        private IPEndPoint Endpoint;
        private NetworkStream Stream;
        private long Sequence;
        private List<ISerfMessageHandler> Handlers = new List<ISerfMessageHandler>();

        /// <summary>
        /// Occurs when the client was successfulyl connected to the remote agent.
        /// </summary>
        public event SerfConnectEvent Connected;

        /// <summary>
        /// Occurs when the client was disconnected from the remote agent.
        /// </summary>
        public event SerfConnectEvent Disconnected;

        /// <summary>
        /// Gets whether the client is currently connected or not.
        /// </summary>
        public bool IsActive 
        {
            get; 
            private set;
        }

        /// <summary>
        /// Connects to the remote agent and starts listening to events.
        /// </summary>
        public async void Connect(IPEndPoint endpoint, string authKey, TimeSpan timeout)
        {
            // Remember the endpoint
            this.Endpoint = endpoint;

            // Set a default timeout
            if (timeout == default(TimeSpan))
                timeout = TimeSpan.FromSeconds(10);

            // Create a TCP Client
            this.Client = new TcpClient();
            this.Client.SendTimeout = (int)timeout.TotalMilliseconds;
            this.Client.ReceiveTimeout = (int)timeout.TotalMilliseconds;

            // Connect to the remote 
            await this.Client.ConnectAsync(this.Endpoint.Address, this.Endpoint.Port);

            // Get the network stream for reading & writing
            this.Stream = this.Client.GetStream();

            // Setup the handshake
            this.Handshake();

            // Invoke connected event
            if (this.Connected != null)
                this.Connected(this);

            // Start receiving
            await this.Receive();
        }

        /// <summary>
        /// Listens and reads from the socket.
        /// </summary>
        /// <returns></returns>
        private async Task Receive()
        {
            this.IsActive = true;
            var buffer = new byte[8192];

            using (var stream = new MemoryStream())
            {
                while (this.IsActive)
                {
                    try
                    {
                        // If content is null, that means the connection has been gracefully disconnected.
                        var length = await this.Stream.ReadAsync(buffer, 0, 8192);
                        if (length == 0)
                        {
                            this.OnDisconnect();
                            return;
                        }

                        // Write to pending buffer
                        stream.Write(buffer, 0, length);
                        stream.Seek(0, SeekOrigin.Begin);

                        // Process pending data while we have data to process.
                        while (stream.Position < stream.Length)
                            this.ProcessPending(stream);

                        // Reset the stream
                        stream.Position = 0;
                    }
                    catch (IOException)
                    {
                        // If the tcp connection is ungracefully disconnected, it will throw an exception
                        this.OnDisconnect();
                        return;
                    }
                }
            }

        }

        /// <summary>
        /// Processes a pending message from the underlying stream.
        /// </summary>
        /// <param name="stream">The stream to handle.</param>
        private void ProcessPending(MemoryStream stream)
        {
            var packer = MessagePackSerializer.Get<ResponseHeader>();
            var header = packer.Unpack(stream);

            // Seek for a handler of the sequence (if any)
            for (int i = 0; i < this.Handlers.Count; ++i )
            {
                var handler = this.Handlers[i];
                if(handler.Seq == header.Seq)
                {
                    // Remove the handler
                    this.Handlers.RemoveAt(i);

                    // Process the response and return
                    handler.Process(stream);
                    return;
                }
            }
        }


        #region Private Members
        /// <summary>
        /// GetSeq returns the next sequence number in a safe manner.
        /// </summary>
        private ulong GetSeq()
        {
            // Increment the sequence and return it
            return (ulong)Interlocked.Increment(ref this.Sequence);
        }

        /// <summary>
        /// Invoked on disconnect.
        /// </summary>
        private void OnDisconnect()
        {
            this.IsActive = false;
            if (Disconnected != null)
                Disconnected(this);
        }

        /// <summary>
        /// Setups a handler for the response.
        /// </summary>
        /// <typeparam name="TResponse">The response type to set the handler for.</typeparam>
        /// <param name="header">The header of the operation.</param>
        /// <param name="onComplete">The handler to register</param>
        private void RegisterHandler<TResponse>(RequestHeader header, Action<TResponse> onComplete)
        {
            // Prepare the handler
            var handler = new SerfMessageHandler<TResponse>();
            handler.Seq = header.Seq;
            handler.Handler = onComplete;

            // Add the handler to the list
            this.Handlers.Add(handler);
        }


        private void Send<TRequest, TResponse>(RequestHeader header, TRequest request, Action<TResponse> onComplete)
        {
            // Register response handler
            this.RegisterHandler<TResponse>(header, onComplete);

            // Send a header + request
            this.Send<TRequest>(header, request);
        }

        private void Send<TResponse>(RequestHeader header,  Action<TResponse> onComplete)
        {
            // Register response handler
            this.RegisterHandler<TResponse>(header, onComplete);

            // Send a header + request
            this.Send(header);
        }

        private void Send<TRequest>(RequestHeader header, TRequest request)
        {
            // Pack the header and the body
            var packer1 = MessagePackSerializer.Get<RequestHeader>();
            var packer2 = MessagePackSerializer.Get<TRequest>();
            packer1.Pack(this.Stream, header);
            packer2.Pack(this.Stream, request);
            this.Stream.Flush();
        }

        private void Send(RequestHeader header)
        {
            // Pack the header only
            var packer1 = MessagePackSerializer.Get<RequestHeader>();
            packer1.Pack(this.Stream, header);
            this.Stream.Flush();
        }
        #endregion

        #region Static Members
                /// <summary>
        /// Gets the serialization context to use for unpacking.
        /// </summary>
        public static readonly SerializationContext Context;

        /// <summary>
        /// Static constructor
        /// </summary>
        static SerfClient()
        {
            // Create our custom context
            SerfClient.Context = new SerializationContext();
            SerfClient.Context.Serializers
                .Register<IPAddress>(new IPAddressSerializer(SerfClient.Context));
        }
        #endregion

        public void Handshake()
        {
            // Construct a new request header.
            var header = new RequestHeader()
            {
                Command = SerfCommand.Handshake,
                Seq = this.GetSeq()
            };

            var req = new HandshakeRequest()
            {
                Version = 1
            };

            this.Send<HandshakeRequest>(header, req);
        }

        public void GetMembers(Action<MembersResponse> onComplete)
        {
            // Construct a new request header.
            var header = new RequestHeader()
            {
                Command = SerfCommand.Members,
                Seq = this.GetSeq()
            };


            // Send the request
            this.Send<MembersResponse>(header, onComplete);
        }

    }
}
