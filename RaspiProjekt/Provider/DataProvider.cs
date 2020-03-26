using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using MonitoreCore.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MonitoreCore.Provider.DataProvider
{
    public class RaspiProvider : IDataProvider
    {
        public RaspiProvider()
        {
            this.Initialize();

        }

        private void Initialize()
        {

        }

        public int GetAnalogDataFromSPI(int channel)
        {
            var value = default(object);
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
                Console.WriteLine($"Fehler: {ex}");
            }

            return Convert.ToInt32(value);
        }

        public bool GetDigitalData(int pin)
        {
            throw new NotImplementedException();
        }

        public void WriteDititalData(int pin, bool value)
        {
            throw new NotImplementedException();
        }
    }
}
