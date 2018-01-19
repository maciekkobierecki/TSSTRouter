using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tsst_client;
using System.Threading.Tasks;
using static LabelSwitchingRouter.FIB;
using LabelSwtichingRouter;
using CustomSocket;

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
            LogClass.WhiteLog("MPLSPacket added to inPort " + portNumber);           
            Program.count++;
            ChangeLabel(mplsPacket);
            return mplsPacket;
        }
        public ThreadSafeList<MPLSPacket> ProcessPack(MPLSPack mplsPack, int destPort)
        {
            LogClass.WhiteLog("MPLSPack added to inPort " + destPort);           
            ThreadSafeList<MPLSPacket> packets = UnpackPack(mplsPack);
            LogClass.WhiteLog("MPLSPack unpacked, comutating MPLSPackets");            

            foreach (MPLSPacket packet in packets) {
                Program.count++;
                packet.DestinationPort = destPort;
            }
            packets.ForEach(ChangeLabel);
            return packets;
        }

        public void UpdateFIB(List<Entry> table)
        {
            fib.UpdateRoutingTable(table);
            LogClass.Log("Updating FIB in inPort " + portNumber);
            fib.DisplayFIB(portNumber);
        }


        public ThreadSafeList<MPLSPacket> UnpackPack(MPLSPack pack)
        {
            return pack.Unpack();
        }

        private void ChangeLabel(MPLSPacket packet)
        {
            int oldPort = packet.DestinationPort;
            int oldLabel = packet.GetLabelFromStack();

            int[] FIBOutput = fib.GetOutput(oldPort, oldLabel, packet.ipPacket.destinationAddress);
            int port = FIBOutput[0];
            int label = FIBOutput[1];
            packet.DestinationPort = port;
            LogClass.WhiteLog("MPLSPacket from inPort " + oldPort + ": old label = " + oldLabel);
            if (label != 0)
            {
                packet.PutLabelOnStack(label);
                Console.WriteLine("            | new label = " + label);
            }
            else Console.WriteLine("            | old label removed");                      

            if (fib.LookForLabelToBeAdded(oldPort, oldLabel) != 0)
            {
                int addingLabel = fib.LookForLabelToBeAdded(oldPort, oldLabel);
                packet.PutLabelOnStack(fib.LookForLabelToBeAdded(oldPort, oldLabel));
                LogClass.Log("Starting new tunnel with label " + addingLabel);                
                ChangeLabel(packet);
            }
            else if (fib.LookForLabelToBeRemoved(oldPort, oldLabel) != 0)
            {
                LogClass.Log("Ending tunnel");                
                ChangeLabel(packet);
            }
            
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




