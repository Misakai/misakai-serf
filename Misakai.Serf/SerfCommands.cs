using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misakai.Serf
{
    /// <summary>
    /// The list of commands.
    /// </summary>
    public static class SerfCommand
    {
        public const string Handshake = "handshake";
        public const string Event = "event";
        public const string ForceLeave = "force-leave";
        public const string Join = "join";
        public const string Members = "members";
        public const string MembersFiltered = "members-filtered";
        public const string Stream = "stream";
        public const string Stop = "stop";
        public const string Monitor = "monitor";
        public const string Leave = "leave";
        public const string InstallKey = "install-key";
        public const string UseKey = "use-key";
        public const string RemoveKey = "remove-key";
        public const string ListKeys = "list-keys";
        public const string Tags = "tags";
        public const string Query = "query";
        public const string Respond = "respond";
        public const string Auth = "auth";
        public const string Stats = "stats";

    }
}
