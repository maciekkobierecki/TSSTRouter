using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomSocket;

namespace LabelSwitchingRouter
{
    class FIB
    {
        public List<Entry> routingTable;

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
            if (!routingTable.Contains(FindInputDestination(inport, inlabel, address)))
            {
                routingTable.Add(new Entry(inport, inlabel, outport, outlabel, newlabel, removelabel, address));
                LogClass.Log("Added new entry in FIB: inport " + inport + " inlabel " + inlabel + " outport " + outport + " outlabel " + outlabel + " | destinationAddress: " + address);
            }
            else LogClass.WhiteLog("Entry with such input parameters already exists. Delete it before adding new one."); 
        }

        public void RemoveEntry(int inport, int inlabel)
        {
            Entry entryToBeDeleted = FindInput(inport, inlabel);
            if (entryToBeDeleted != null)
            {
                routingTable.Remove(entryToBeDeleted);
                LogClass.MagentaLog("Deleted entry from FIB: inport "+ entryToBeDeleted.InPort + " inlabel " + entryToBeDeleted.InLabel + 
                    " outport " + entryToBeDeleted.OutPort + " outlabel " + entryToBeDeleted.OutLabel + " removed from FIB.");                
            }
            else LogClass.WhiteLog("Entry with such input parameters doesn't exist in this FIB."); 
        }
         
        public List<Entry> ReturnSubTable(int inport)
        {
            return routingTable.FindAll(x => x.InPort == inport);
        }

        private Entry FindInput(int iport, int ilabel)
        {
            return routingTable.FindAll(x => x.InPort == iport).Find(y => y.InLabel == ilabel);
        }

        private Entry FindInputDestination(int iport, int ilabel, string destination)
        {
            return routingTable.FindAll(x => x.InPort == iport).FindAll(y => y.InLabel == ilabel).Find(z => z.IPAddress == destination);
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
            foreach (Entry entry in routingTable) {
                LogClass.WhiteLog("[FIB] InPort " + entry.InPort + " InLabel " + entry.InLabel + " OutPort " + entry.OutPort + " OutLabel " + entry.OutLabel  + " DestinationAddress " + entry.IPAddress);                     
            }

        }

    }
}
