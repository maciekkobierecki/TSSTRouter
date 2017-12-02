using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tsst_client;
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
        public MPLSPacket ProcessPacket(MPLSPacket mplsPacket)
        {
            Console.WriteLine("MPLSPacket added to inPort {0}", portNumber);

            ChangeLabel(mplsPacket);
            return mplsPacket;
        }
        public List<MPLSPacket> ProcessPack(MPLSPack mplsPack, int destPort)
        {
            Console.WriteLine("MPLSPack with label {0} added to inPort {1}", destPort, portNumber);
            List<MPLSPacket> packets = UnpackPack(mplsPack);
            foreach (MPLSPacket packet in packets) {
                packet.DestinationPort = destPort;
            }
            packets.ForEach(ChangeLabel);
            return packets;
        }

        public void UpdateFIB(List<Entry> table)
        {
            fib.UpdateRoutingTable(table);
            Console.WriteLine("Updating FIB in inPort {0}", portNumber);
        }


        public List<MPLSPacket> UnpackPack(MPLSPack pack)
        {
            return pack.Unpack();
        }


        private void ChangeLabel(MPLSPacket packet)
        {
            int oldPort = packet.DestinationPort;
            int oldLabel = packet.GetLabelFromStack();

            int[] FIBOutput = fib.GetOutput(oldPort, oldLabel);
            int port = FIBOutput[0];
            int label = FIBOutput[1];
            packet.DestinationPort = port;
            if (label != 0) packet.PutLabelOnStack(label);

            if (fib.LookForLabelToBeAdded(oldPort, oldLabel) != 0)
            {
                packet.PutLabelOnStack(fib.LookForLabelToBeAdded(oldPort, oldLabel));
                ChangeLabel(packet);
            }
            else if (fib.LookForLabelToBeRemoved(oldPort, oldLabel) != 0)
            {
                ChangeLabel(packet);
            }
            Console.WriteLine("MPLSPacket label changed from {0} to {1}", oldLabel, label);
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

        }

        public MPLSPack PrepareMPLSPackFromBuffer()
        {
            MPLSPack pack = new MPLSPack(packetBuffer);
            pack.DestinationPort = packetBuffer[0].DestinationPort;
            return pack;
        }
        public void BufferClear()
        {
            packetBuffer.Clear();
        }

        public Packet PrepareIPPacketFromBuffer(int bufferPosition)
        {
            Packet ipPacket = packetBuffer[bufferPosition].ipPacket;
            packetBuffer.RemoveAt(bufferPosition);
            return ipPacket;
        }

        public int GetBufferLength()
        {
            return packetBuffer.Count();
        }

        public int GetPortNumber()
        {
            return portNumber;
        }

        public bool SendingToClient()
        {
            if (packetBuffer.Count > 0)
                if (packetBuffer[0].labelStack.Count == 0) return true;
            return false;
        }

        public delegate void packIsReadyDelegate();
        public event packIsReadyDelegate SendPackage;

    }

}




