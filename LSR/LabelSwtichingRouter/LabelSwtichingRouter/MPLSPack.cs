using LabelSwtichingRouter;
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
        private ThreadSafeList<MPLSPacket> packets;
        public int DestinationPort { get; set; }

        public MPLSPack(ThreadSafeList<MPLSPacket> packets)
        {
            this.packets = packets;
        }

        public MPLSPack()
        {
        }

        public ThreadSafeList<MPLSPacket> Unpack()
        {
            return packets;
        }

    }
}
