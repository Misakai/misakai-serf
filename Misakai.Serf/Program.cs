using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Misakai.Serf
{
    class Program
    {
        static void Main(string[] args)
        {
            var authKey = GetKey();
            var client = new SerfClient();
            client.Connected += OnConnect;
            client.Connect(IPAddress.Parse("130.211.86.34"), 7373, "test");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void OnConnect(SerfClient client)
        {
            client.GetMembers((members) =>
            {
                foreach (var member in members.Members)
                {
                    Console.WriteLine("{0}:{1} {2}", member.Addr, member.Port, member.Name);
                    foreach(var tag in member.Tags)
                    {
                        Console.WriteLine("   tag: {0} = {1}", tag.Key, tag.Value);
                    }
                }
            });
        }

        static string GetKey()
        {
            var key = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            return Convert.ToBase64String(key);
        }
    }
}
