using System;
using System.Collections.Generic;
using System.Linq;
using tsst_client;
using System.Text;
using System.Threading.Tasks;
using static LabelSwitchingRouter.FIB;

namespace LabelSwitchingRouter
{
    class InPort
    {
        protected int portNumber;
        private FIB fib;

        public InPort(int portNumber, List<Entry> routingTable)
        {
            this.portNumber = portNumber;
            fib = new FIB(routingTable);
        }

        public MPLSPacket ProcessPacket(Packet packet)
        {
            MPLSPacket labeledMPLS = SetLabel(packet);
            return labeledMPLS;
        }

        public List<MPLSPacket> ProcessPack(MPLSPack mplsPack)
        {
            List<MPLSPacket> packets = UnpackPack(mplsPack);
            packets.ForEach(ChangeLabel);
            return packets;
        }

        public void UpdateFIB(List<Entry> table) {
            fib.UpdateRoutingTable(table);

        }


        public List<MPLSPacket> UnpackPack(MPLSPack pack)
        {
            return pack.Unpack();
        }

        private MPLSPacket SetLabel(Packet packet)
        {
            int label = fib.ExchangeIpAddressForLabel(packet.destinationAddress);
            MPLSPacket mplspacket = new MPLSPacket(packet, label);
            mplspacket.DestinationPort = mplspacket.ipPacket.destinationPort;
            ChangeLabel(mplspacket);
            return mplspacket;
        }

        private void ChangeLabel(MPLSPacket packet)
        {
            int oldPort = packet.DestinationPort;
            int oldLabel = packet.GetLabelFromStack();
            int[] FIBOutput = fib.GetOutput(oldPort, oldLabel);
            int port = FIBOutput[0];
            int label = FIBOutput[1];
            packet.DestinationPort = port;
            packet.PutLabelOnStack(label);

            if (fib.LookForLabelToBeAdded(oldPort, oldLabel) != 0)
            {
                packet.PutLabelOnStack(fib.LookForLabelToBeAdded(oldPort, oldLabel));
            }
            if (fib.LookForLabelToBeRemoved(oldPort, oldLabel) == oldLabel)
            {
                packet.RemoveTopLabelFromStack();
            }
        }

        private void EndMPLSTunnel(MPLSPacket packet)
        {
            packet.RemoveTopLabelFromStack();
        }

        public int GetPortNumber()
        {
            return portNumber;
        }
    }

    class OutPort
    {
        protected int portNumber;
        protected List<MPLSPacket> packetBuffer;

        public OutPort(int number)
        {
            portNumber = number;
            packetBuffer = new List<MPLSPacket>();
        }

        public void AddToBuffer(MPLSPacket packet)
        {
            packetBuffer.Add(packet);
            Console.WriteLine("Packet with label {0} has been added to buffer of outPort {1}", packet.labelStack.Peek(), portNumber);

        }

        public MPLSPack PrepareMPLSPackFromBuffer()
        {
            MPLSPack pack = new MPLSPack(packetBuffer);
            pack.DestinationPort = packetBuffer[0].DestinationPort;
            packetBuffer.Clear();
            return pack;
        }

        public Packet PrepareIPPacketFromBuffer(int bufferPosition)
        {
            Packet ipPacket = packetBuffer[bufferPosition].ipPacket;
            packetBuffer.RemoveAt(bufferPosition);
            return ipPacket;
        }

        public int GetPortNumber()
        {
            return portNumber;
        }

        public delegate void packIsReadyDelegate();
        public event packIsReadyDelegate SendPackage;

    }

}




