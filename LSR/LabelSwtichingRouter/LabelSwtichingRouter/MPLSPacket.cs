using System;
using System.Collections.Generic;
using System.Linq;
using tsst_client;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    [Serializable]
    class MPLSPacket
    {
        public Packet ipPacket;
        public int DestinationPort { get; set; }
        public Stack<int> labelStack;

        public MPLSPacket(Packet ipPacket, int label)
        {
            this.ipPacket = ipPacket;
            labelStack = new Stack<int>();
            labelStack.Push(label);
        }
        public MPLSPacket(MPLSPacket packet)
        {
            ipPacket = new Packet(packet.ipPacket);
            labelStack = new Stack<int>();
            for (int i = 0; i < packet.labelStack.Count; i++)
                labelStack.Push(packet.labelStack.ElementAt(i));
            DestinationPort = packet.DestinationPort;
        }

        public int GetLabelFromStack()
        {
            return labelStack.Pop();
        }

        public void PutLabelOnStack(int l)
        {
            labelStack.Push(l);
        }

        public void RemoveTopLabelFromStack()
        {
            labelStack.Pop();
        }
    }
}
