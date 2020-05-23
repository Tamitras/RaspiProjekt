using MonitoreCore.Enum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                if (logger == null)
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

        public void WriteToConsole(string text, bool value = false)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now + " ");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"{text} {value}");
        }

        public void WriteToFile(LogType logType, string msg)
        {
            var basePath = "/home/pi/Logs/";
            var logFilePath = basePath + "Log";
            var saveFilePath = logFilePath + "_" + DateTime.Now;
            msg += Environment.NewLine;

            try
            {
                //TODO: Muss überarbeitet werden

                // Nicht jeden Log in das selbe File schreiben
                // 1. Liefere alle Files im Ordner ("/home/pi/Logs")
                // 2. Liefere das aktuelleste File
                // 3. Prüfe ob das Erstelldatum "heute" ist
                // 3.1 Ja -> Benutze es und schreibe hinein
                // 3.2 Nein -> Erstelle ein neues mit dem heutigem Datum

                if (Directory.Exists(basePath))
                {
                    // Directory exisiert
                }
                else
                {
                    // exisitiert nicht
                    Directory.CreateDirectory(basePath);
                }

                if (File.Exists(logFilePath))
                {
                    FileInfo fileInfo = new FileInfo(logFilePath);
                    if(fileInfo.CreationTime < DateTime.Today)
                    {
                        File.Move(logFilePath, saveFilePath);
                        File.Create(logFilePath).Dispose();
                    }
                }
                else
                {
                    File.Create(logFilePath).Dispose();
                }

                    var textToWrite = DateTime.Now + " ";
                    switch (logType)
                    {
                        case LogType.Info:
                            textToWrite += LogType.Info;
                            break;
                        case LogType.Warning:
                            textToWrite += LogType.Warning;
                            break;
                        case LogType.Error:
                            textToWrite += LogType.Error;
                            break;
                        default:
                            break;
                    }

                    textToWrite += " " + msg;

                    File.AppendAllText(logFilePath, textToWrite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler: {ex}");
            }
        }
    }
}
