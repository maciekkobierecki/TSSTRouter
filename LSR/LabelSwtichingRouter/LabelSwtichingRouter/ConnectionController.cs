using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Subnetwork;

namespace LabelSwitchingRouter
{
    class ConnectionController
    {
        private FIB fib;
        private List<(int port, string address)> addressTranslation;
        public ConnectionController(FIB fib)
        {
            this.fib = fib;
            addressTranslation.Add((1, "192.0.3.2"));
            addressTranslation.Add((2, "192.0.3.8"));
            addressTranslation.Add((3, "192.0.3.7"));
            addressTranslation.Add((4, "192.0.3.5"));
            addressTranslation.Add((5, "192.0.3.1"));
            addressTranslation.Add((6, "192.0.3.5"));
            addressTranslation.Add((7, "192.0.3.4"));
        }

        private bool ConnectionRequestIn(SNP pathBegin, SNP pathEnd)
        {
            int inputPort=0, outputPort=0;
            foreach ((int port, string address) translation in addressTranslation)
            {
                if (translation.address == pathBegin.Address)
                {
                    inputPort = translation.port;
                }
                else if (translation.address == pathEnd.Address)
                {
                    outputPort = translation.port;
                }
            }

            fib.AddEntry(inputPort, pathBegin.Label, outputPort, pathEnd.Label, 0, 0, "0");
           
            return true;

        }
    }
}
