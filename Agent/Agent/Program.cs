using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    public class Program
    {
        static void Main(string[] args)
        {
            RouterAgent router = new RouterAgent();
            router.Listen();
        }
    }
}
