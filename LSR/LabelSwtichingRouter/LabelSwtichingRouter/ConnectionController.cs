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
            addressTranslation = new List<string[]>();
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

        public bool ConnectionRequestIn(SNP firstSNP, SNP secondSNP)
        {
            int inputPort = 0, outputPort = 0;

            foreach (string[] translation in addressTranslation)
            {
                if (translation[0] == firstSNP.Address)
                {
                    inputPort = Int32.Parse(translation[1]);
                }
                else if (translation[0] == secondSNP.Address)
                {
                    outputPort = Int32.Parse(translation[1]);
                }
            }

            if (firstSNP.Label == 0)
            {
                foreach (string[] translation in addressTranslation)
                {
                    if (translation[0] == firstSNP.PathBegin)
                    {
                        fib.AddEntry(inputPort, firstSNP.Label, outputPort, secondSNP.Label, 0, 0, firstSNP.PathEnd);
                        fib.AddEntry(outputPort, secondSNP.Label, inputPort, firstSNP.Label, 0, 0, "0");
                    }
                    else if (translation[0] == firstSNP.PathEnd)
                    {
                        fib.AddEntry(inputPort, firstSNP.Label, outputPort, secondSNP.Label, 0, 0, firstSNP.PathBegin);
                        fib.AddEntry(outputPort, secondSNP.Label, inputPort, firstSNP.Label, 0, 0, "0");
                    }
                }
            }
            else if (secondSNP.Label == 0)
            {
                foreach (string[] translation in addressTranslation)
                {
                    if (translation[0] == secondSNP.PathBegin)
                    {
                        fib.AddEntry(inputPort, firstSNP.Label, outputPort, secondSNP.Label, 0, 0, secondSNP.PathEnd);
                        fib.AddEntry(outputPort, secondSNP.Label, inputPort, firstSNP.Label, 0, 0, "0");
                    }
                    else if (translation[0] == secondSNP.PathEnd)
                    {
                        fib.AddEntry(inputPort, firstSNP.Label, outputPort, secondSNP.Label, 0, 0, secondSNP.PathBegin);
                        fib.AddEntry(outputPort, secondSNP.Label, inputPort, firstSNP.Label, 0, 0, "0");
                    }
                }
            }
            else
            {
                fib.AddEntry(inputPort, firstSNP.Label, outputPort, secondSNP.Label, 0, 0, "0");
                fib.AddEntry(outputPort, secondSNP.Label, inputPort, firstSNP.Label, 0, 0, "0");
            }
            return true;
        }
    }
}


