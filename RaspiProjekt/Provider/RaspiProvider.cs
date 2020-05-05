using MonitoreCore.Debug;
using MonitoreCore.Enum;
using MonitoreCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi.Native;

namespace MonitoreCore.Provider.DataProvider
{
    public class RaspiProvider : IRaspiProvider
    {
        public static GpioController Controller { get; set; } = new GpioController();
        private Log Logger { get; set; } = new Log();

        private static bool AutoMode { get; set; } = true;

        private static System.Timers.Timer MyTimer { get; set; } = new System.Timers.Timer(20000);

        public RaspiProvider()
        {
            MyTimer.Elapsed += MyTimer_Elapsed;
        }

        private void MyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now}: Automatik Modus Prüfung");
            Console.WriteLine($"{DateTime.Now}: Automode: <{AutoMode}>");
            if (AutoMode)
            {
                this.StartAutoMode();
            }
        }

        private void StartAutoMode()
        {
            int lichtwert = this.GetAnalogDataFromSPI(0);
            int feuchtigkeitswert = this.GetAnalogDataFromSPI(1);
            int pumpenPinWert = this.GetPumpenPinStatus();

            bool lichtIO = false;
            bool feuchtigkeitIO = false;
            bool pumpenStatusIO = false;

            if (lichtwert > 20000)
            {
                lichtIO = true;
            }
            if (feuchtigkeitswert > 50000 && feuchtigkeitswert != 1337)
            {
                feuchtigkeitIO = true;
            }
            if (pumpenPinWert == 0)
            {
                pumpenStatusIO = true;
            }

            if (lichtIO && feuchtigkeitIO && pumpenStatusIO)
            {
                this.PumpeAn();
            }
            else
            {
                Console.WriteLine("Pumpe nicht angesteuert, weil:");

                if (!lichtIO)
                {
                    Console.WriteLine("Lichtwert zu Hell");
                }
                if (!feuchtigkeitIO)
                {
                    Console.WriteLine("Feuchtigkeit zu Nass");
                }
                if (!pumpenStatusIO)
                {
                    Console.WriteLine("Pumpe läuft bereits");
                }
            }
        }

        public int GetAnalogDataFromSPI(int channel)
        {
            var value = default(string);
            var ret = default(int);

            try
            {
                // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader($"/home/pi/ErikPython/Channel{channel}.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    value = sr.ReadToEnd();

                    if (string.IsNullOrEmpty(value))
                    {
                        return 1337;
                    }
                    else
                    {
                        ret = Convert.ToInt32(value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler: {ex}");
            }

            return ret;
        }

        /// <summary>
        /// Setz den Wert des Pins und somit High oder Low
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        public bool WriteDigitalData(int pin, int value)
        {
            try
            {
                this.OpenPinAndSetModeToOutput(pin);
                WiringPi.DigitalWrite(pin, value);
                AutoMode = false;

                if (value == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.Logger.WriteToFile(LogType.Error, $"Fehler beim Setzen des Pins: {ex.Message}");
                return false;
            }
        }

        public int GetPumpenPinStatus()
        {
            try
            {
                return WiringPi.DigitalRead(23);
            }
            catch (Exception ex)
            {
                this.Logger.WriteToFile(LogType.Error, $"Fehler beim Setzen des Pins: {ex.Message}");
                return 1;
            }
        }

        private void OpenPinAndSetModeToOutput(int pin)
        {
            if (!Controller.IsPinOpen(pin))
            {
                Controller.OpenPin(pin);
                Controller.SetPinMode(pin,PinMode.Output);
            }
        }

        private void PumpeAn()
        {
            Console.WriteLine($"{DateTime.Now}: (1) Pumpe wurde vom MainThread angestoßen");
            new Task(() =>
            {
                Console.WriteLine($"{DateTime.Now}: (2) Pumpe wird von ausgelagerten Thread angeschalten");
                WriteDigitalData(23, 1);
                Thread.Sleep(5000);
                this.PumpeAus();
                Console.WriteLine($"{DateTime.Now}: (3) Pumpe wurde von ausgelagerten Thread ausgeschlten");

            }).Start();

            Console.WriteLine($"{DateTime.Now}: (4) MainThread läuft einfach weiter");
        }

        private void PumpeAus()
        {
            WriteDigitalData(23, 0);
        }

        public bool SetAutoMode(int value)
        {
            if (value == 1)
            {
                AutoMode = true;
                MyTimer.Start();
            }
            else
            {
                AutoMode = false;
                MyTimer.Stop();
            }
            return AutoMode;
        }
    }
}
