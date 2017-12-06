using LabelSwitchingRouter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    class InputManager
    {
        public const int INTEGER_SIZE = 4;

        private Socket inputSocket;

        public delegate void ReceivedDelegate(object oSender, object packet, int destPort);
        public event ReceivedDelegate ProcessPackage;

        public InputManager()
        {
            initalizeInputSocket();
        }
        private void initalizeInputSocket()
        {
            int port = Config.getIntegerProperty("CableCloudOutPort");
            String address = Config.getProperty("CableCloudAddress");
            createSocket(address, port);
        }
        private void createSocket(String address, int port)
        {
            IPAddress cableCloudAddress = IPAddress.Parse(address);
            IPEndPoint ipe = new IPEndPoint(cableCloudAddress, port);
            inputSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            inputSocket.Connect(ipe);
        }
        public void waitForInput()
        {
            int objectSize = ReadIncomingObjectSize();
            int destPort = ReadIncomingObjectSize(); //tak naprawde to to jest dest 
            removeSourcePortInformation();
            objectSize = decreaseObjectSizeByPortNumber(objectSize);
            object receivedObject = ReceiveObject(objectSize);
            //Console.WriteLine("Received object from CableCloud");
            FireRecievedEvent(receivedObject, destPort);
        }

        private void removeSourcePortInformation()
        {
            byte[] bytes = new byte[4];
            inputSocket.Receive(bytes, 0, INTEGER_SIZE, SocketFlags.None);
        }

        private int decreaseObjectSizeByPortNumber(int objectSize)
        {
            return objectSize - 2*INTEGER_SIZE;
        }
        private int ReadIncomingObjectSize()
        {
            byte[] objectSize = new byte[4];
            inputSocket.Receive(objectSize, 0, INTEGER_SIZE, SocketFlags.None);
            int size = BitConverter.ToInt32(objectSize, 0);
            return size;
        }

        private object ReceiveObject(int inputSize)
        {
            byte[] bytes = new byte[inputSize];
            int totalReceived = 0;
            do
            {
                int received = inputSocket.Receive(bytes, totalReceived, inputSize - totalReceived, SocketFlags.Partial);
                totalReceived += received;
            } while (totalReceived != inputSize);
            object o = Deserialize(bytes);
            return o;
        }
        private object Deserialize(byte[] serializedObject)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            ms.Write(serializedObject, 0, serializedObject.Length);
            ms.Seek(0, SeekOrigin.Begin);
            object deserialized = bf.Deserialize(ms);
            return deserialized;

        }
        public void FireRecievedEvent(Object receivedObject, int destPort)
        {
            if (null != ProcessPackage)
                ProcessPackage(this, receivedObject, destPort);
        }
    }
}
