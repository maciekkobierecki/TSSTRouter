﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    class FIB
    {
        private List<Entry> routingTable;

        public class Entry
        {
            public int InPort { get; set; }
            public int OutPort { get; set; }
            public int InLabel { get; set; }
            public int OutLabel { get; set; }
            public int NewLabel { get; set; }
            public int RemoveLabel { get; set; }
            public String IPAddress { get; set; }

            public Entry(int ip, int il, int op, int ol, int nl, int rl, String address)
            {
                InPort = ip;
                OutPort = op;
                InLabel = il;
                OutLabel = ol;
                NewLabel = nl;
                RemoveLabel = rl;
                IPAddress = address;
            }
        }

        public FIB()
        {
            routingTable = new List<Entry>();
        }

        public FIB(List<Entry> rtable) : this()
        {
            routingTable = rtable;
        }

        public void UpdatePortsRoutingTables(List<InPort> ports)
        {
            foreach (InPort port in ports) {
                int inPort = port.GetPortNumber();
                port.UpdateFIB(ReturnSubTable(inPort));
            }
        }

        public void UpdateRoutingTable(List<Entry> routingTable)
        {
            this.routingTable = routingTable;
        }

        public void AddEntry(int inport, int inlabel, int outport, int outlabel, int newlabel, int removelabel, String address)
        {
            if (!routingTable.Contains(FindInput(inport, inlabel)))
            {
                routingTable.Add(new Entry(inport, inlabel, outport, outlabel, newlabel, removelabel, address));
                Console.WriteLine("Added new entry in FIB: inport {0} inlabel {1} outport {2} outlabel {3}", inport, inlabel, outport, outlabel);
            }
            else Console.WriteLine("Entry with such input parameters already exists. Delete it before adding new one.");
        }

        public void RemoveEntry(int inport, int inlabel)
        {
            Entry entryToBeDeleted = FindInput(inport, inlabel);
            if (entryToBeDeleted != null)
            {
                routingTable.Remove(entryToBeDeleted);
                Console.WriteLine("Deleted entry from FIB: inport {0} inlabel {1} outport {2} outlabel {3} removed from FIB.",
                    entryToBeDeleted.InPort, entryToBeDeleted.InLabel, entryToBeDeleted.OutPort, entryToBeDeleted.OutLabel);
            }
            else Console.WriteLine("Entry with such input parameters doesn't exist in this FIB.");
        }
         
        public List<Entry> ReturnSubTable(int inport)
        {
            return routingTable.FindAll(x => x.InPort == inport);
        }

        private Entry FindInput(int iport, int ilabel)
        {
            return routingTable.FindAll(x => x.InPort == iport).Find(y => y.InLabel == ilabel);
        }

        public int[] GetOutput(int iport, int ilabel)
        {
            try
            {
                Entry result = routingTable.FindAll(x => x.InPort == iport).Find(x => x.InLabel == ilabel);
                int[] outPair = { result.OutPort, result.OutLabel };
                return outPair;
            }
            catch(Exception e)
            {
                int[] error = { 0, 0 };
                return error;
            }

        }

        public int LookForLabelToBeAdded(int iport, int ilabel)
        {
            Entry result = FindInput(iport, ilabel);
            int label = result.NewLabel;
            return label;
        }

        public int LookForLabelToBeRemoved(int iport, int ilabel)
        {
            Entry result = FindInput(iport, ilabel);
            int label = result.RemoveLabel;
            return label;
        }

        public int ExchangeIpAddressForLabel(String ipaddress, int inPort)
        {
            Entry result = routingTable.FindAll(x => x.IPAddress == ipaddress).Find(x => x.InPort == inPort);
            int label = result.InLabel;
            return label;
        }

        public void DisplayFIB(int inPortNumber) {
            Console.WriteLine("{0} | FIB of inPort {1}", DateTime.Now.ToString("h:mm:ss tt"), inPortNumber);
            foreach (Entry entry in routingTable) {
                Console.WriteLine("InPort {0} InLabel {1} OutPort {2} OutLabel {3} NewLabel {4} RemoveLabel {5} DestinationAddress {6}", 
                    entry.InPort, entry.InLabel, entry.OutPort, entry.OutLabel, entry.NewLabel, entry.RemoveLabel, entry.IPAddress);
            }

        }

    }
}
