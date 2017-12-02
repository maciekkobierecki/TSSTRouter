using LabelSwitchingRouter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using tsst_client;

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
            sendingTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            sendingTimer.Enabled = true;
            agent = new RouterAgent(fib, inPorts);
            CreateInPorts(numberOfInputModules);
            CreateOutPorts(numberOfOutputModules);
           Console.WriteLine("Created LSR");

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
            }

        }

        private void CreateInPorts(int numberOfInputPorts)
        {
            for (int i = 1; i <= numberOfInputPorts; i++)
            {
                int portNumber = int.Parse(Config.getProperty("InPortNumber" + i));
                InPort inPort = new InPort(portNumber, fib.ReturnSubTable(portNumber));
                inPorts.Add(inPort);
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            sendingTimer.Stop();
            foreach (OutPort outPort in outPorts)
            {
                if (outPort.GetBufferLength() > 0)
                {
                    Console.WriteLine(outPort.GetBufferLength());
                    if (outPort.SendingToClient())
                    {
                        int bufferLength = outPort.GetBufferLength();
                        for(int i=0; i<bufferLength; i++)
                        {
                            Packet bufferObject = outPort.PrepareIPPacketFromBuffer(0);
                            OutputManager.sendIPPacket(bufferObject, outPort, outPort.GetPortNumber());
                        }
                    }
                    else
                    {
                        MPLSPack bufferContent = outPort.PrepareMPLSPackFromBuffer();
                        OutputManager.sendMPLSPack(bufferContent, outPort.GetPortNumber(), outPort);
                    }
                }
            }
            sendingTimer.Start();
        }

        public void PassToInModule(object oSender, object received, int destPort)
        {
            try
            {
                InPort inPort;
                int destinationPort;
                if (received.GetType() == typeof(Packet))
                {
                    Packet receivedPacket = (Packet)received;
                    MPLSPacket packet = SetLabelAndPort(receivedPacket, destPort);
                    destinationPort = GetPortNumber(packet);
                    inPort = GetInPort(destinationPort);
                    MPLSPacket processedPacket = inPort.ProcessPacket(packet);
                    Commutate(processedPacket);
                    Console.WriteLine("Passing Packet to inPort {0}", destinationPort);
                }
                else if (received.GetType() == typeof(MPLSPack))
                {
                    MPLSPack receivedPack = (MPLSPack)received;
                    destinationPort = destPort;
                    inPort = GetInPort(destinationPort);
                    List<MPLSPacket> processedPackets = inPort.ProcessPack(receivedPack, destPort);
                    foreach (MPLSPacket packet in processedPackets)
                    {
                        Commutate(packet);
                        Console.WriteLine("Passing MPLSPack to inPort {0}", destinationPort);
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine("Connection doesn't exist");
            }
           
        }

        private MPLSPacket SetLabelAndPort(Packet packet, int destinationPort)
        {
            int label = fib.ExchangeIpAddressForLabel(packet.destinationAddress, destinationPort);
            int inPort = fib.ExchangeIpAddressForPort(packet.destinationAddress, destinationPort);
            MPLSPacket mplspacket = new MPLSPacket(packet, label);
            mplspacket.DestinationPort = inPort;
            return mplspacket;
        }

        private int GetPortNumber(MPLSPacket receivedPacket)
        {
            int portNumber = receivedPacket.DestinationPort;
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
