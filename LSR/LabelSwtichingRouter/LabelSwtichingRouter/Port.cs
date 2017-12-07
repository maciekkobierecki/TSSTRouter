using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tsst_client;
using System.Threading.Tasks;
using static LabelSwitchingRouter.FIB;
using LabelSwtichingRouter;

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
            Program.count++;
            Program.print();
            ChangeLabel(mplsPacket);
            return mplsPacket;
        }
        public ThreadSafeList<MPLSPacket> ProcessPack(MPLSPack mplsPack, int destPort)
        {
            Console.WriteLine("{0} | MPLSPack with label {0} added to inPort {1}", DateTime.Now.ToString("h: mm: ss tt"), destPort, portNumber);
            ThreadSafeList<MPLSPacket> packets = UnpackPack(mplsPack);
            Console.WriteLine("{0} | MPLSPack unpacked, comutating MPLSPackets", DateTime.Now.ToString("h: mm: ss tt"));

            foreach (MPLSPacket packet in packets) {
                Program.count++;
                Program.print();
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


        public ThreadSafeList<MPLSPacket> UnpackPack(MPLSPack pack)
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
                Console.WriteLine("{0} | Starting new tunnel", DateTime.Now.ToString("h: mm: ss tt"));
                ChangeLabel(packet);
            }
            else if (fib.LookForLabelToBeRemoved(oldPort, oldLabel) != 0)
            {
                Console.WriteLine("{0} | Ending tunnel", DateTime.Now.ToString("h: mm: ss tt"));
                ChangeLabel(packet);
            }
            Console.WriteLine("{0} | MPLSPacket label from inPort {1} changed from {2} to {3}, will be sent to outPort {4}", DateTime.Now.ToString("h: mm: ss tt"), oldPort, oldLabel, label, port);
        }

        public int GetPortNumber()
        {
            return portNumber;
        }
    }

    class OutPort
    {
        protected int portNumber;
        protected ThreadSafeList<MPLSPacket> packetBuffer;

        public OutPort(int number)
        {
            portNumber = number;
            packetBuffer = new ThreadSafeList<MPLSPacket>();
        }

        public void AddToBuffer(MPLSPacket packet)
        {
            packetBuffer.Add(packet);

        }

        public MPLSPack PrepareMPLSPackFromBuffer()
        {
            ThreadSafeList<MPLSPacket> currentBuffer = packetBuffer;
            packetBuffer = new ThreadSafeList<MPLSPacket>();
            MPLSPack pack = new MPLSPack(currentBuffer);
            pack.DestinationPort = currentBuffer[0].DestinationPort;
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
            if (packetBuffer.Count() > 0)
                if (packetBuffer[0].labelStack.Count == 0) return true;
            return false;
        }

        public delegate void packIsReadyDelegate();
        public event packIsReadyDelegate SendPackage;

    }

}




