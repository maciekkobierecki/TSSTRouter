using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    [Serializable]
    class MPLSPack
    {
        private List<MPLSPacket> packets;
        public int DestinationPort { get; set; }

        public MPLSPack(List<MPLSPacket> packets)
        {
            this.packets = packets;
        }


        public List<MPLSPacket> Unpack()
        {
            return packets;
        }

    }
}
