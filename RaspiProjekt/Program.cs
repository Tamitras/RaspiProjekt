using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using MonitoreCore.Provider.DataProvider;
using MonitoreCore.WebServer;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;
using Unosquare.WiringPi.Native;

namespace MonitoreCore
{
    public class Program
    {
        public static System.Device.Gpio.GpioController Controller { get; set; } = new System.Device.Gpio.GpioController();

        public static string IpAdress { get; set; }

        static void Main(string[] args)
        {
            //WaitForDebuggingAttatched();
            Pi.Init<BootstrapWiringPi>();

            int port;
            IpAdress = Initialize(out port);

            // WebServer
            StartWebServer(args, port);

            ReadGPIOPin();

            Console.ReadLine();
        }

        private static void ReadGPIOPin()
        {
            //var pin = 2; // Entspricht Pin Belegung 3 auf dem Raspi
            var pin = 23; // Entspricht Pin Belegung 16 auf dem Raspi

            if (!Controller.IsPinOpen(pin))
            {
                Controller.OpenPin(pin);
                if (Controller.GetPinMode(pin) != PinMode.Output)
                {
                    Controller.SetPinMode(pin, System.Device.Gpio.PinMode.Output);
                }
            }
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

        private static string Initialize(out int port)
        {
            var hostName = Dns.GetHostName();
            var ipAdress = "";
            port = 5000;
            if (hostName.Contains("rasp"))
            {
                // raspberry
                var ipAdressesRaw = GetIpAdressesByBashCommand("hostname -I");
                ipAdress = GetIpAdressByRawIpAdresses(ipAdressesRaw);
            }
            else
            {
                // windows
                var ip = $"http://localhost:{port}";
                var adresses = Dns.GetHostAddresses(hostName);
                var ipAdresses = adresses.Where(c => c.AddressFamily == AddressFamily.InterNetwork).ToList();
                ipAdress = ipAdresses.FirstOrDefault().ToString();
            }

            return ipAdress;
        }

        private static void StartWebServer(string[] args, int port)
        {
            var t = new Task(() =>
            {
                try
                {
                    Console.WriteLine("WebServer starting");
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
                    Console.WriteLine($"Server bereits gestartet :)");
                    throw ex;
                }
            });

            t.Start();
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
    }
}
