using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using MonitoreCore.Provider.DataProvider;
using MonitoreCore.WebServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace MonitoreCore
{
    public class Program
    {
        public static System.Device.Gpio.GpioController Controller { get; set; } = new System.Device.Gpio.GpioController();

        static void Main(string[] args)
        {
            //WaitForDebuggingAttatched();
            var dataProvider = new RaspiProvider();

            Pi.Init<BootstrapWiringPi>();

            var keepMonitoring = true;
            string IpAdress;
            int port;
            Initialize(out IpAdress, out port);

            // Enable only on Raspberry
            //GetRaspBiInformation();

            //GetGpioInfo();


            // WebServer
            StartWebServer(args, port);

            // Temperaturüberwachung
            StartTemperaturUeberwachung(keepMonitoring, IpAdress, port);

            Console.ReadLine();
        }

        /// <summary>
        /// If this Method is enabled
        /// The Compiler breaks and waits until Debugger is attached
        /// </summary>
        private static void WaitForDebuggingAttatched()
        {
            Console.WriteLine("Waiting for debugger to attach");
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }
            Console.WriteLine("Debugger attached");
        }

        private static void GetGpioInfo()
        {
            var pinCount = Controller.PinCount;
            Console.WriteLine($"PinCount: {pinCount}");

            //GetAndReadAllPins();
            Console.WriteLine(Pi.Info);
        }

        private static List<int> GetAndReadAllPins()
        {
            var pins = new List<int>();
            for (int i = 0; i < 28; i++)
            {
                pins.Add(i);
            }

            foreach (var pin in pins)
            {
                try
                {
                    if (Controller.IsPinOpen(pin))
                    {
                        var val = Controller.Read(pin);
                        Console.WriteLine($"Pin<{pin + 1}> geöffnet. Value: {val}");
                    }
                    else
                    {
                        //Controller.OpenPin(pin, PinMode.Input);
                        var val = Controller.Read(pin);
                        Console.WriteLine($"Pin<{pin + 1}> geschlossen - wird geöffnet. Value: {val}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler bei Pin: {pin + 1}: {ex}");
                }
            }

            return pins;
        }

        private static void Initialize(out string IpAdress, out int port)
        {
            var hostName = Dns.GetHostName();
            IpAdress = "";
            port = 5000;
            if (hostName.Contains("rasp"))
            {
                // raspberry
                var ipAdressesRaw = GetIpAdressesByBashCommand("hostname -I");
                IpAdress = GetIpAdressByRawIpAdresses(ipAdressesRaw);
            }
            else
            {
                // windows
                var ip = $"http://localhost:{port}";
                var adresses = Dns.GetHostAddresses(hostName);
                var ipAdresses = adresses.Where(c => c.AddressFamily == AddressFamily.InterNetwork).ToList();
                IpAdress = ipAdresses.FirstOrDefault().ToString();
            }
        }

        private static void StartWebServer(string[] args, int port)
        {
            var t = new Task(() =>
            {
                try
                {
                    WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()

                    // Diese Einstellung beschränkt den Server auf gewisse IpAdressen
                    //.UseUrls("http://0.0.0.0:5000", $"http://{IpAdress}:5000", "http://*:5000")

                    // WIchtig (/*) bedeutet, dass der Raspberry Pi nicht nur "localhost" hört und somit im gesamten Netzwerk erreichbar ist.
                    .UseUrls($"http://*:{port}")
                    .UseEnvironment("Development")
                    .UseEnvironment("ASPNETCORE_URLS")
                    .Build()
                    .Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"EXCEPTION: {ex.Message}");
                    throw ex;
                }
            });

            t.Start();
        }

        private static void StartTemperaturUeberwachung(bool keepMonitoring, string IpAdress, int port)
        {
            var t2 = new Task(() =>
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Thread.Sleep(2500);
                Console.WriteLine("-------------------------------------------------");
                Thread.Sleep(1500);
                Console.WriteLine($"Reachable at: {IpAdress}:{port}");
                Thread.Sleep(1500);
                Console.WriteLine($"For Example : {IpAdress}:{port}/Main/Register");
                Thread.Sleep(1500);
                Console.WriteLine($"For Example : {IpAdress}:{port}/Main/Hello?ipAdress={"localhost"}&hostname={"Erik"}");
                Thread.Sleep(1500);
                Console.WriteLine($"For Example : {IpAdress}:{port}/Main/GetChannel?Channel={"0"}");
                Console.WriteLine("-------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;

                ShowsTemperatureInConsole(keepMonitoring);
            });

            t2.Start();
        }

        private static string GetIpAdressByRawIpAdresses(string ipAdressesRaw)
        {
            var ipAdress = ipAdressesRaw.Split(" ").FirstOrDefault();
            if (ipAdress == null)
            {
                throw new Exception("IP Adresse konnte nicht über 'Bash Call <hostname -I>' ermittelt werden ");
            }

            return ipAdress;
        }

        private static string GetIpAdressesByBashCommand(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }

        private static void ShowsTemperatureInConsole(bool keepMonitoring)
        {
            new Task(() =>
            {
                // Get Temperature
                string path = @"/sys/class/thermal/thermal_zone0/temp";

                while (keepMonitoring)
                {
                    Thread.Sleep(2000);

                    if (!File.Exists(path))
                    {
                        Console.WriteLine("Could not find " + path);
                    }
                    else
                    {
                        string readText = File.ReadAllText(path);
                        if (Double.TryParse(readText, out Double temp))
                        {
                            if ((temp / 1000) >= 28 && (temp / 1000) < 30)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            }
                            else if ((temp / 1000) >= 30 && (temp / 1000) < 32)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                            else if ((temp / 1000) >= 32)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                            }

                            Console.WriteLine($"CPU Tempteratur: {temp / 1000}°C");
                        }
                        else
                        {
                            Console.WriteLine("Parsing hat nicht funktioniert :/");
                        }
                    }

                }
            }).Start();
        }

        private static void GetRaspBiInformation()
        {
            Console.WriteLine("Informationen über den Raspi");

            Console.WriteLine($"BoardModel: {Pi.Info.BoardModel}");
            Console.WriteLine($"BoardRevision: {Pi.Info.BoardRevision}");
            Console.WriteLine($"CpuArchitecture: {Pi.Info.CpuArchitecture}");
            Console.WriteLine($"CpuImplementer: {Pi.Info.CpuImplementer}");
            Console.WriteLine($"CpuPart: {Pi.Info.CpuPart}");
            Console.WriteLine($"CpuRevision: {Pi.Info.CpuRevision}");
            Console.WriteLine($"CpuVariant: {Pi.Info.CpuVariant}");
            Console.WriteLine($"Hardware: {Pi.Info.Hardware}");
            Console.WriteLine($"InstalledRam: {Pi.Info.InstalledRam}");
            Console.WriteLine($"IsLittleEndian: {Pi.Info.IsLittleEndian}");
            Console.WriteLine($"LibraryVersion: {Pi.Info.LibraryVersion}");
            Console.WriteLine($"Manufacturer: {Pi.Info.Manufacturer}");
            Console.WriteLine($"ModelName: {Pi.Info.ModelName}");
            Console.WriteLine($"MemorySize: {Pi.Info.MemorySize}");
            Console.WriteLine($"OperatingSystem: {Pi.Info.OperatingSystem}");
            Console.WriteLine($"ProcessorCount: {Pi.Info.ProcessorCount}");
            Console.WriteLine($"ProcessorModel: {Pi.Info.ProcessorModel}");
            Console.WriteLine($"RaspberryPiVersion: {Pi.Info.RaspberryPiVersion}");
            Console.WriteLine($"Revision: {Pi.Info.Revision}");
            Console.WriteLine($"RevisionNumber: {Pi.Info.RevisionNumber}");
            Console.WriteLine($"Serial: {Pi.Info.Serial}");
            Console.WriteLine($"Uptime: {Pi.Info.Uptime}");
            Console.WriteLine($"UptimeTimeSpan: {Pi.Info.UptimeTimeSpan}");
        }
    }
}
