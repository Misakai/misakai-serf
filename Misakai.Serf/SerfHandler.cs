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
    /// Represents a delegate to invoke on connect/disconnect events.
    /// </summary>
    /// <param name="client"></param>
    public delegate void SerfConnectEvent(SerfClient client);

    /// <summary>
    /// Handlers a message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal struct SerfMessageHandler<T> : ISerfMessageHandler
    {
        public ulong Seq { get; set; }
        public Action<T> Handler;

        /// <summary>
        /// Invokes the handler with the specified response object.
        /// </summary>
        public void Process(MemoryStream stream)
        {
            // Unpack the response with the custom context
            var packer = MessagePackSerializer.Get<T>(SerfClient.Context);
           
            //var response = packer.UnpackSingleObject(buffer);
            var response = packer.Unpack(stream);

            // Invoke the handler with typed parameter
            this.Handler(response);
        }
    }

    internal interface ISerfMessageHandler
    {
        ulong Seq { get; }

        /// <summary>
        /// Invokes the handler with the specified response object.
        /// </summary>
        void Process(MemoryStream stream);
    }
}
