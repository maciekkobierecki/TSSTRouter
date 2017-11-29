using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMS;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Agent
{
    class RouterAgent
    {
        static Socket output_socket = null;
        static Socket inputSocket = null;
        static Socket foreignSocket = null;
        public Command inCommand = new Command();
        public Command outCommand = new Command();
        public int outport;     //port, na którym agent słucha 
                
        public void Listen()
        {
            Console.WriteLine("Type port");
            outport = Int32.Parse(Console.ReadLine());
            SendSingleCommand(outCommand);
            inputSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdd = IPAddress.Parse("127.0.0.1");
            IPEndPoint remoteEP = new IPEndPoint(ipAdd, outport);
            inputSocket.Bind(remoteEP);
            int i = 1;
            while (true)
            {
                inputSocket.Listen(0);
                foreignSocket = inputSocket.Accept();
                byte[] bytes = new byte[foreignSocket.SendBufferSize];
                int readByte = foreignSocket.Receive(bytes);

                Thread t;
                t = new Thread(() =>
                {
                    //W tym uroczym obiekciku znajdują się wszystkie niezbędne parametry do FIBa, nic tylko uzupełniać tablice
                    inCommand = GetDeserializedCommand(bytes);
                    Console.WriteLine(inCommand.inPort + " " + inCommand.inLabel + " " + inCommand.outPort + " " + inCommand.outLabel);
                    //SendSingleCommand(outCommand);
                    i++;
                }
                );
                t.Start();
            }
        }

        // Wysyła info do NMS-a jak wstaje, ten interfejs jest na razie z przedziału A1-A4
        // rozszerzy się to później w zależności ile będzie takich routerów potrzebnych
        private void SendSingleCommand(Command cm)
        {
            Console.WriteLine("Type interface");
            string _interface = Console.ReadLine();
            cm = new Command(_interface, outport, 0, 0, 0, 0);
            Thread tr;
            tr = new Thread(() =>
            {
                Connect();
                output_socket.Send(GetSerializedCommand(cm));
            }
            );
            tr.Start();
        }

        private void Connect()                  //Połączenie
        {
            output_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdd = IPAddress.Parse("127.0.0.1");
            IPEndPoint remoteEP = new IPEndPoint(ipAdd, 7386); 
            output_socket.Connect(remoteEP);
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

        private byte[] GetSerializedCommand(Command com)    //Serializacja bajtowa
        {            
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, com);
            return ms.ToArray();
        }
    }
}
