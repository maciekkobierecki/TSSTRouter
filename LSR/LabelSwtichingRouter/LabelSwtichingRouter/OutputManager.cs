using LabelSwitchingRouter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using tsst_client;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    class OutputManager
    {
        private static Socket outputSocket;
        static int port;
        public static void initialize()
        {
            port = Config.getIntegerProperty("CableCloudInPort");
            String address = Config.getProperty("CableCloudAddress");
            createSocket(address, port);
        }
        private static void createSocket(String address, int port)
        {
            IPAddress ip = IPAddress.Parse(address);
            IPEndPoint ipe = new IPEndPoint(ip, port);
            outputSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            outputSocket.Connect(ipe);
        }
        public static void sendMPLSPack(MPLSPack mplsPack, int outPort, OutPort port)
        {
            byte[] serializedMPLSPack = getSerializedPack(mplsPack);
            int packSize = serializedMPLSPack.Length + 8;
            outputSocket.Send(BitConverter.GetBytes(packSize));
            outputSocket.Send(BitConverter.GetBytes(packSize));
            outputSocket.Send(BitConverter.GetBytes(outPort));
            outputSocket.Send(serializedMPLSPack);
        }

        private static byte[] getSerializedPack(MPLSPack pack)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, pack);
            byte[] serialized = ms.ToArray();
            return serialized;
        }

        public static void sendIPPacket(Packet ipPacket, OutPort port, int outPort)
        {
            byte[] serializedMPLSPack = getSerializedIPPacket(ipPacket);
            int packSize = serializedMPLSPack.Length + 8;

            outputSocket.Send(BitConverter.GetBytes(packSize));
            outputSocket.Send(BitConverter.GetBytes(packSize));
            outputSocket.Send(BitConverter.GetBytes(outPort));
            outputSocket.Send(serializedMPLSPack);
           
        }

        private static byte[] getSerializedIPPacket(Packet ipPacket)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, ipPacket);
            byte[] serialized = ms.ToArray();
            return serialized;
        }

    }
}