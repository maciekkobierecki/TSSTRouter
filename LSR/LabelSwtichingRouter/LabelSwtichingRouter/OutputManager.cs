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

        public static void initialize()
        {
            int port = Config.getIntegerProperty("CableCloudInPort");
            String address = Config.getProperty("CableCloudAddress");
            Socket createdSocket = createSocket(address, port);
        }
        private static Socket createSocket(String address, int port)
        {
            IPAddress ip = IPAddress.Parse(address);
            IPEndPoint ipe = new IPEndPoint(ip, port);
            Socket createdSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            createdSocket.Connect(ipe);
            return createdSocket;
        }
        public static void sendMPLSPack(MPLSPack mplsPack)
        {
            byte[] serializedMPLSPack = getSerializedPack(mplsPack);
            int packSize = serializedMPLSPack.Length;
            outputSocket.Send(BitConverter.GetBytes(packSize));
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

        public static void sendIPPacket(Packet ipPacket)
        {
            byte[] serializedMPLSPack = getSerializedIPPacket(ipPacket);
            int packSize = serializedMPLSPack.Length;
            outputSocket.Send(BitConverter.GetBytes(packSize));
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