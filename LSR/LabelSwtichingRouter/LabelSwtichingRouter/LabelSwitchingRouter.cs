using LabelSwitchingRouter;
using System;
using System.Collections.Generic;
using System.Linq;
using tsst_client;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LabelSwitchingRouter
{
    class LabelSwitchingRouter
    {
        FIB fib;
        List<InPort> inPorts;
        List<OutPort> outPorts;
        Timer sendingTimer;
        RouterAgent agent;
        int numberOfInputModules, numberOfOutputModules;

        public LabelSwitchingRouter()
        {
            fib = new FIB();
            numberOfInputModules = GetInputModulesNumber();
            numberOfOutputModules = GetOutputModulesNumber();
            inPorts = new List<InPort>();
            outPorts = new List<OutPort>();
            sendingTimer = new Timer();
            sendingTimer.Interval = Config.getIntegerProperty("SendingInterval");
            //agent = new RouterAgent(fib, inPorts);
            CreateInPorts(numberOfInputModules);
            CreateOutPorts(numberOfOutputModules);
            Console.WriteLine("Stworzono LSR.");

        }

        private int GetInputModulesNumber()
        {
            int number = Config.getIntegerProperty("NumberOfInputPorts");
            return number;
        }
        private int GetOutputModulesNumber()
        {
            int number = Config.getIntegerProperty("NumberOfOutputPorts");
            return number;

        }

        private void CreateOutPorts(int numberOfOutputPorts)
        {
            for (int i = 1; i <= numberOfOutputPorts; i++)
            {
                int portNumber = Config.getIntegerProperty("OutPortNumber" + i);
                OutPort outPort = new OutPort(portNumber);
                outPorts.Add(outPort);
                Console.WriteLine("Created outPort no. " + i);
            }

        }

        private void CreateInPorts(int numberOfInputPorts)
        {
            for (int i = 1; i <= numberOfInputPorts; i++)
            {
                int portNumber = int.Parse(Config.getProperty("InPortNumber" + i));
                InPort inPort = new InPort(portNumber, fib.ReturnSubTable(portNumber));
                inPorts.Add(inPort);
                Console.WriteLine("Created inPort no. " + portNumber);
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            OutPort outPort = null;
            for (int i = 0; i < outPorts.Count; i++)
            {
                outPort = (OutPort)outPorts.ElementAt(i);
                MPLSPack bufferContent = outPort.PrepareMPLSPackFromBuffer();
                OutputManager.sendMPLSPack(bufferContent);
            }
        }

        public void PassToInModule(object oSender, object received)
        {
            InPort inPort;
            int destinationPort;
            if (received.GetType() == typeof(Packet))
            {
                Packet receivedPacket = (Packet)received;
                destinationPort = GetPortNumber(receivedPacket);
                inPort = GetInPort(destinationPort);
                MPLSPacket processedPacket = inPort.ProcessPacket(receivedPacket);
                Commutate(processedPacket);
            }
            else if (received.GetType() == typeof(MPLSPack))
            {
                MPLSPack receivedPack = (MPLSPack)received;
                destinationPort = receivedPack.DestinationPort;
                inPort = GetInPort(destinationPort);
                List<MPLSPacket> processedPackets = inPort.ProcessPack(receivedPack);
                foreach (MPLSPacket packet in processedPackets) {
                    Commutate(packet);
                } 
            }
        }

        private int GetPortNumber(Packet receivedPacket)
        {
            int portNumber = receivedPacket.destinationPort;
            return portNumber;
        }

        private InPort GetInPort(int portNumber)
        {
            foreach (InPort port in inPorts)
            {
                int comparedPortNumber = port.GetPortNumber();
                if (comparedPortNumber == portNumber)
                    return port;
            }
            return null;
        }

        private void Commutate(MPLSPacket packet)
        {
            int packetOutPort = packet.DestinationPort;
            int portNumber;
            foreach (OutPort port in outPorts)
            {
                portNumber = port.GetPortNumber();
                if (packetOutPort == portNumber)
                {
                    port.AddToBuffer(packet);
                    return;
                }
            }
        }
        
    }
}
