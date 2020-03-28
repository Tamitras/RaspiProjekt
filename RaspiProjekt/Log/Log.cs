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

        public void WriteToConsole(string text, int value)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now + " ");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"{text} {value}");
        }

        public void WriteToConsole(string text, bool value)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now + " ");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"{text} {value}");
        }
    }
}
