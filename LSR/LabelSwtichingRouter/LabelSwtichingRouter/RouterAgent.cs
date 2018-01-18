﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMS;
using System.Timers;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CustomSocket;

namespace LabelSwitchingRouter
{
    class RouterAgent
    {
        private Socket output_socket = null;
        private Socket inputSocket = null;
        private Socket foreignSocket = null;
        public Command inCommand, outCommand;
        private List<InPort> inPorts;
        private FIB fib;
        private int outport;
        private string _interface;
        private Boolean connected;

        public RouterAgent(FIB fib, List<InPort> inPorts)
        {
            this.fib = fib;
            inCommand = new Command();
            outCommand = new Command();
            this.inPorts = inPorts;
            _interface = Config.getProperty("NMSInterface");
            outport = Config.getIntegerProperty("NMSListenPort");
            init();
            LogClass.WhiteLog("Established connection with NMS");           
            SendSingleCommand(_interface, outport);
            LogClass.WhiteLog("Sent hello message");

            inputSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdd = IPAddress.Parse("127.0.0.1");
            IPEndPoint remoteEP = new IPEndPoint(ipAdd, outport);
            inputSocket.Bind(remoteEP);

            Listen();
        }


        private void SendSingleCommand(string agentID, int agentPort)
        {
            Command cm = new Command(agentID, agentPort);
            byte[] serializedCommand = GetSerializedCommand(cm);
            int messageSize = serializedCommand.Length;
            byte[] size = BitConverter.GetBytes(messageSize);
            output_socket.Send(size);
            output_socket.Send(serializedCommand);

        }

        private void SendKeepAliveMessage(string agentID)
        {
            Command cm = new Command(agentID);
            if (!connected)
                init();
           

            Thread tr;
            tr = new Thread(() =>
            {
                byte[] serialized = GetSerializedCommand(cm);
                int serializedSize = serialized.Length;
                byte[] sizeBytes = BitConverter.GetBytes(serializedSize);
                output_socket.Send(sizeBytes);
                output_socket.Send(serialized);
            });
            tr.Start();
        }

        private void SendFib(Command cm)
        {            
            byte[] serializedCommand = GetSerializedCommand(cm);
            int messageSize = serializedCommand.Length;
            byte[] size = BitConverter.GetBytes(messageSize);
            output_socket.Send(size);
            output_socket.Send(serializedCommand);
        }

        private void init()
        {
            output_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdd = IPAddress.Parse("127.0.0.1");
            IPEndPoint remoteEP = new IPEndPoint(ipAdd, 7386);
            output_socket.Connect(remoteEP);
            connected = true;
        }

        public void Listen()
        {
            inputSocket.Listen(0);
            Thread t;
            t = new Thread(() =>
            {
                    foreignSocket = inputSocket.Accept();
                while (true)
                {
                    byte[] bytes=Receive(foreignSocket);
                    inCommand = GetDeserializedCommand(bytes);
                    if (inCommand.agentId == "Fib")
                    {
                        foreach (FIB.Entry entry in fib.routingTable)
                        {
                            Command kom = new Command("Fib", Config.getIntegerProperty("NMSListenPort"), entry.InPort, entry.InLabel, entry.OutPort, entry.OutLabel, entry.NewLabel, entry.RemoveLabel, entry.IPAddress);
                            SendFib(kom);                            
                        }
                    }
                    else if (inCommand.agentId == "Add")
                    {
                        fib.AddEntry(inCommand.inPort, inCommand.inLabel, inCommand.outPort, inCommand.outLabel, inCommand.newLabel, inCommand.removeLabel, inCommand.ipAdress);
                        fib.UpdatePortsRoutingTables(inPorts);
                    }
                    else
                    {
                        fib.RemoveEntry(inCommand.inPort, inCommand.inLabel);
                        fib.UpdatePortsRoutingTables(inPorts);
                    }
                    
                }
            }
            );
            t.Start();

            /*Thread tr;
            tr = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(3000);
                    SendKeepAliveMessage("KeepAlive");
                }
            }
            );
            tr.Start();
            */

        }

        private byte[] Receive(Socket inputSocket)
        {
            byte[] messageSize = new byte[4];
            inputSocket.Receive(messageSize, 0, 4, SocketFlags.None);
            int inputSize = BitConverter.ToInt32(messageSize,0);
            byte[] bytes = new byte[inputSize];
            int totalReceived = 0;
            do
            {
                int received = inputSocket.Receive(bytes, totalReceived, inputSize - totalReceived, SocketFlags.Partial);
                totalReceived += received;
            } while (totalReceived != inputSize);
            return bytes;
        }

        private Command GetDeserializedCommand(byte[] b)
        {
            Command c = new Command();
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(b, 0, b.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            c = (Command)binForm.Deserialize(memStream);
            return c;
        }

        private byte[] GetSerializedCommand(Command com)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, com);
            return ms.ToArray();
        }
    }
}