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
        private List<string[]> addressTranslation; //IPaddress - localPort
        public ConnectionController(FIB fib)
        {
            this.fib = fib;
            LoadTransationTableFromFile();
        }

        private void LoadTransationTableFromFile()
        {
            string fileName = Config.getProperty("AddressTranslationTableFileName");
            string[] loadedFile = LoadFile(fileName);
            string[] snppParams = null;
            foreach (string str in loadedFile)
            {
                snppParams = str.Split(' ');
                addressTranslation.Add(snppParams);
            }
        }

        private string[] LoadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }

        private bool ConnectionRequestIn(SNP pathBegin, SNP pathEnd)
        {
            int inputPort=0, outputPort=0;
            foreach (string[] translation in addressTranslation)
            {
                if (translation[0] == pathBegin.Address)
                {
                    inputPort = Int32.Parse(translation[1]);
                }
                else if (translation[0] == pathEnd.Address)
                {
                    outputPort = Int32.Parse(translation[1]);
                }
            }
            fib.AddEntry(inputPort, pathBegin.Label, outputPort, pathEnd.Label, 0, 0, "0");
            return true;
        }
    }
}

           
