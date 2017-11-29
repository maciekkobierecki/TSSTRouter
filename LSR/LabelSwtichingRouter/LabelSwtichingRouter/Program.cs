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
        static void Main(string[] args)
        {
            Console.WriteLine("Halo");
            OutputManager.initialize();
            InputManager inputManager = new InputManager();
            LabelSwitchingRouter lsr = new LabelSwitchingRouter();
            inputManager.ProcessPackage += lsr.PassToInModule;
            Thread t = new Thread(new ParameterizedThreadStart(listenForInput));
            t.Start(inputManager);
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
