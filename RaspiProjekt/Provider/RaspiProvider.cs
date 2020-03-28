using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using MonitoreCore.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unosquare.WiringPi.Native;

namespace MonitoreCore.Provider.DataProvider
{
    public class RaspiProvider : IRaspiProvider
    {
        public static System.Device.Gpio.GpioController Controller { get; set; } = new System.Device.Gpio.GpioController();

        public RaspiProvider()
        {
            this.Initialize();
        }

        private void Initialize()
        {

        }

        public int GetAnalogDataFromSPI(int channel, out string exception)
        {
            var value = default(string);
            exception = default(string);

            try
            {
                // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader($"/home/pi/ErikPython/Channel{channel}.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    value = sr.ReadToEnd();
                }

                //Console.WriteLine($"Value: {value}");
            }
            catch (Exception ex)
            {
                exception = ex.ToString();
                Console.WriteLine($"Fehler: {ex}");
            }

            if (string.IsNullOrEmpty(value))
            {
                return 1337;
            }
            else
            {
                return Convert.ToInt32(value);
            }
        }

        public bool GetDigitalData(int pin)
        {
            throw new NotImplementedException();
        }

        public bool WriteDititalData(int pin, int value)
        {
            var pinset = false;
            try
            {
                if (Controller.IsPinOpen(pin))
                {
                    Controller.SetPinMode(pin, System.Device.Gpio.PinMode.Output);
                    WiringPi.DigitalWrite(pin, value);
                    pinset = true;
                }
                else
                {

                    Controller.OpenPin(pin);
                    Controller.SetPinMode(pin, System.Device.Gpio.PinMode.Output);
                    WiringPi.DigitalWrite(pin, value);
                    pinset = true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            if (pinset)
            {
                if (value == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new Exception($"Fehler bei setzen des Pins{pin}");
            }
        }
    }
}
