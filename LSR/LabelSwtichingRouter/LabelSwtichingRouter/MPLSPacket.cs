using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tsst_client;
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
