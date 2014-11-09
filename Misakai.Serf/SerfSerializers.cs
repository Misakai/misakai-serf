using MsgPack;
using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Misakai.Serf
{
    public class IPAddressSerializer : MessagePackSerializer<IPAddress>
    {

        public IPAddressSerializer(SerializationContext context)
            : base(context)
        {

        }

        protected override void PackToCore(Packer packer, IPAddress objectTree)
        {
            throw new NotImplementedException();
        }

        protected override IPAddress UnpackFromCore(Unpacker unpacker)
        {
            // Read IP Address
            var buffer = unpacker.LastReadData.AsBinary();
            return new IPAddress(buffer);
            //return IPAddress.Any;
        }
    }
}
