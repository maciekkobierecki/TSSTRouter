using CustomSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Subnetwork;
using System.Threading.Tasks;
using System.Net;
using System.Threading;


namespace LabelSwtichingRouter
{
    class ParentSubnetworkConnector
    {
        public const String OPERATED_SUBNETWORK = "SubnetworkAddress";
        public const String OPERATED_SUBNETWORK_MASK = "SubnetworkMask";
        public const String PARENT_SUBNETWORK = "ParentSubnetworkAddress";
        public const String PARENT_SUBNETWORK_PORT = "ParentSubnetworkPort";
        public const String CONNECTION_REQEST_FROM_CC = "connectionRequest";
        public const String DELETE_CONNECTION_REQUEST = "deleteRequest";


        private static LabelSwitchingRouter.ConnectionController connectionController;
        private static CSocket toParentSocket;
        private static SubnetworkAddress mySubnetAddress;

        public static void init(LabelSwitchingRouter.ConnectionController cc)
        {
            connectionController = cc;
            String subnetAddress = Config.getProperty(OPERATED_SUBNETWORK);
            String mySubnetMask = Config.getProperty(OPERATED_SUBNETWORK_MASK);
            mySubnetAddress = new SubnetworkAddress(subnetAddress, mySubnetMask);
            String parentSubnetworkAddress = Config.getProperty(PARENT_SUBNETWORK);
            if (parentSubnetworkAddress != null)
            {
                int parentSubnetworkPort = Config.getIntegerProperty(PARENT_SUBNETWORK_PORT);
                ConnectToParentSubnetwork(IPAddress.Parse(parentSubnetworkAddress), parentSubnetworkPort);
                SendMySubnetworkInformation();
            }
        }

        private static void ConnectToParentSubnetwork(IPAddress parentSubnetworkAddress, int parentSubnetworkPort)
        {
            toParentSocket = new CSocket(parentSubnetworkAddress, parentSubnetworkPort, CSocket.CONNECT_FUNCTION);
        }

        private static void SendMySubnetworkInformation()
        {
            object toSend = getSubnetworkInformation();
            toParentSocket.SendObject(OPERATED_SUBNETWORK, toSend);
            waitForInputFromSocketInAnotherThread(toParentSocket);

        }

        private static object getSubnetworkInformation()
        {
            Dictionary<string, string> mySubnetworkInformation = new Dictionary<string, string>();
            string mySubnetworkAddress = Config.getProperty(OPERATED_SUBNETWORK);
            string mySubnetworkMask = Config.getProperty(OPERATED_SUBNETWORK_MASK);
            SubnetworkAddress address = new SubnetworkAddress(mySubnetworkAddress, mySubnetworkMask);
            mySubnetworkInformation.Add(OPERATED_SUBNETWORK, mySubnetworkAddress);
            mySubnetworkInformation.Add(OPERATED_SUBNETWORK_MASK, mySubnetworkMask);
            return mySubnetworkInformation;
        }

        private static void waitForInputFromSocketInAnotherThread(CSocket connected)
        {
            var t = new Thread(() => waitForInput(connected));
            t.Start();
        }

        private static void waitForInput(CSocket connected)
        {
            while (true)
            {
                Tuple<String, Object> received = connected.ReceiveObject();
                String parameter = received.Item1;
                Object receivedObject = received.Item2;
                if (parameter.Equals(CONNECTION_REQEST_FROM_CC) || parameter.Equals(DELETE_CONNECTION_REQUEST))
                {
                    Tuple<SNP, SNP> pathToAssign = (Tuple<SNP, SNP>)received.Item2;
                    SNP first = pathToAssign.Item1;
                    SNP second = pathToAssign.Item2;
                    if (!first.Deleting)
                        LogClass.Log("Received request to SET CONNECTION between " + first.Address + " and " + second.Address);
                    else
                        LogClass.Log("Received request to DELETE CONNECTION between " + first.Address + " and " + second.Address);
                    bool response = connectionController.ConnectionRequestIn(pathToAssign.Item1, pathToAssign.Item2);
                    connected.SendACK();
                    LogClass.Log("[ACK] Sending confirmation to Connection Controller");
                }
            }
        }


    }
}
