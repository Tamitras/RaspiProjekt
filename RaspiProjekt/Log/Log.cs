using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MonitoreCore.Debug
{
    public class Log
    {
        private Log logger;
        public Log Logger
        {
            get
            {
                return logger;
            }
            set
            {
                logger = new Log();
            }
        }

        public void WriteToConsole(string value)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now + " ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Ausgelesener Wert(Lichtsensor):{value}");
        }

        public void WriteToConsole(int value)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now + " ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Ausgelesener Wert(Lichtsensor):{value}");
        }
    }
}
