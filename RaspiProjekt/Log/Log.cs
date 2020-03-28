using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MonitoreCore.Debug
{
    public class Log
    {
        private static Log logger;
        public static Log Logger
        {
            get
            {
                if(logger == null)
                {
                    logger = new Log();
                }
                return logger;
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
