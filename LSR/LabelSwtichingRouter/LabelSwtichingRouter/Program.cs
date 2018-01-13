using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabelSwitchingRouter;
using System.Threading;

namespace LabelSwitchingRouter
{
    class Program
    {
        public static int count = 0;
        static void Main(string[] args)
        {
            Console.Title = ("Label Switching Router : " + Config.getProperty("SubnetworkAddress"));
            OutputManager.initialize();
            InputManager inputManager = new InputManager();
            LabelSwitchingRouter lsr = new LabelSwitchingRouter();
            inputManager.ProcessPackage += lsr.PassToInModule;
            while(true)
                inputManager.waitForInput();
            //Thread t = new Thread(new ParameterizedThreadStart(listenForInput));
            //t.Start(inputManager);
        }

        public static void listenForInput(object manager)
        {
            InputManager inputManager = (InputManager)manager;
            while (true)
            {
                inputManager.waitForInput();
            }
        }

    }
}
