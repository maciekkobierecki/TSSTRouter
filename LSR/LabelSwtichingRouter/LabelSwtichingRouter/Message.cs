using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    [Serializable]
    public class Message
    {
        public string s;
        public int inti;
        public int port;

        public Message(string str, int i, int p)
        {
            this.s = str;
            this.inti = i;
            this.port = p;
        }

        public Message()
        {
        }

    }
}
