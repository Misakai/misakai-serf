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
            // 7946
            var endpoint = new IPEndPoint(IPAddress.Parse("130.211.86.34"), 7373);

            var client = new SerfClient();
            client.Connected += OnConnect;
      
            client.Connect(endpoint, null, default(TimeSpan));



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
    }
}
