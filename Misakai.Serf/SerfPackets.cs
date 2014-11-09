using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Misakai.Serf
{

    /// <summary>
    /// Request header is sent before each request.
    /// </summary>
    public struct RequestHeader
    {
        public string Command;
        public ulong Seq;
    }

    /// <summary>
    /// Response header is sent before each response.
    /// </summary>
    public struct ResponseHeader
    {
        public ulong Seq;
        public string Error;
    }

    /// <summary>
    /// Represents a handshake request.
    /// </summary>
    public struct HandshakeRequest
    {
        public int Version;
    }

    /// <summary>
    /// Represents a response for members request.
    /// </summary>
    public struct MembersResponse
    {
        public Member[] Members;
    }

    /// <summary>
    /// Represents an authentication request, performed right after a handshake.
    /// </summary>
    public struct AuthRequest
    {
        public string AuthKey;
    }

    /// <summary>
    /// Member is used to represent a single member of the
    /// Serf cluster.
    /// </summary>
    public class Member
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name;

        /// <summary>
        /// Address of the Serf node
        /// </summary>
        public IPAddress Addr;

        /// <summary>
        /// Gossip port used by Serf
        /// </summary>
        public ushort Port;

        /// <summary>
        ///  The list of tags for the node
        /// </summary>
        public IDictionary<string, string> Tags;

        /// <summary>
        /// Current node status
        /// </summary>
        public string Status;

        /// <summary>
        /// Minimum supported Memberlist protocol
        /// </summary>
        public byte ProtocolMin;

        /// <summary>
        /// Maximum supported Memberlist protocol
        /// </summary>
        public byte ProtocolMax;

        /// <summary>
        /// Currently set Memberlist protocol
        /// </summary>
        public byte ProtocolCur;

        /// <summary>
        /// Minimum supported Serf protocol
        /// </summary>
        public byte DelegateMin;

        /// <summary>
        /// Maximum supported Serf protocol
        /// </summary>
        public byte DelegateMax;

        /// <summary>
        /// Currently set Serf protocol
        /// </summary>
        public byte DelegateCur;
    }

}
