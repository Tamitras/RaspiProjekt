using MonitoreCore.Debug;
using MonitoreCore.Enum;
using MonitoreCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO;
using System.Text;
using System.Threading;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi.Native;

namespace MonitoreCore.Provider.DataProvider
{
    public class RaspiProvider : IRaspiProvider
    {
        public static GpioController Controller { get; set; } = new GpioController();
        private Log Logger { get; set; } = new Log();

        private TimeSpan PumpenIntervallTimeSpan { get; set; } = new TimeSpan();

        /// <summary>
        /// Pin der Pumpe wird auf X gesetzt - Readonly
        /// </summary>
        private int PumpenPin { get; } = 0;

        public RaspiProvider()
        {
        }

        private void SetPumpenIntervall(PumpenIntervall intervall)
        {
            switch (intervall)
            {
                case PumpenIntervall.Kurz:
                    this.PumpenIntervallTimeSpan = new TimeSpan(0, 0, 10); // Stunden, Minuten, Sekunden
                    break;
                case PumpenIntervall.Mittel:
                    this.PumpenIntervallTimeSpan = new TimeSpan(0, 0, 20); // Stunden, Minuten, Sekunden
                    break;
                case PumpenIntervall.Lang:
                    this.PumpenIntervallTimeSpan = new TimeSpan(0, 0, 30); // Stunden, Minuten, Sekunden
                    break;
                default:
                    break;
            }
        }

        public int GetAnalogDataFromSPI(int channel)
        {
            var ret = default(int);

            // @Erik - Diese Methode nochmal anschauen
            // Eventuell die Werte in eine Datei stecken und nicht jeden Channel in eine extra Datei
            // Python Script anpassen

            try
            {
                // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader($"/home/pi/ErikPython/Channel{channel}.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    string value = sr.ReadToEnd();

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

        public bool WriteDigitalData(int pin, int value)
        {
            // @Erik Diese Methode muss ich mir nochmal anschauen 
            try
            {
                this.OpenPinAndSetModeToOutput(pin);
                WiringPi.DigitalWrite(pin, value);

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
                throw ex;
            }
        }

        private void WasserMarsch(PumpenIntervall intervall, out string message)
        {
            message = string.Empty;

            try
            {
                this.StartEngine(intervall);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void StartEngine(PumpenIntervall intervall)
        {
            // Öffnet den Pin und setzt ihn auf den Modus <Output>
            this.OpenPinAndSetModeToOutput(this.PumpenPin);

            // Setzt die Länge der Zeit, die die Pumpe laufen soll.
            this.SetPumpenIntervall(intervall);

            Controller.RegisterCallbackForPinValueChangedEvent(this.PumpenPin, PinEventTypes.Falling, this.PumpeWurdeAusgeschaltet);

            // Create an AutoResetEvent to signal the timeout threshold in the
            // timer callback has been reached.
            var autoEvent = new AutoResetEvent(false);
            //Timer pumpenTimer = new Timer(this.TimerBeended, autoEvent, 0, this.GetPumpenIntervall());

            // Pumpen-Timer wird gestartet
            Timer pumpenTimer = new Timer(this.TimerBeended, null, new TimeSpan(0, 0, 0), this.GetPumpenIntervall());
            WiringPi.DigitalWrite(this.PumpenPin, 1);

        }

        private TimeSpan GetPumpenIntervall()
        {
            return this.PumpenIntervallTimeSpan;
        }

        public void TimerBeended(object state)
        {
            // Pumpe ausschalten
            if (Controller.IsPinOpen(this.PumpenPin))
            {
                var mode = Controller.GetPinMode(this.PumpenPin);
                if (mode != PinMode.Output)
                {
                    WiringPi.DigitalWrite(this.PumpenPin, 0);
                    Controller.Write(this.PumpenPin, PinValue.Low);
                    Console.WriteLine("Pumpe ausgeschalten");
                    Console.WriteLine($"Pumpe lief <{this.GetPumpenIntervall().TotalSeconds}> Sekunden");
                }
            }
        }

        private void PumpeWurdeAusgeschaltet(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            // Pumpe wurde ausgeschalten Event
            // Prüfen ob Pumpe ausgeschalten wurde

            if (Controller.IsPinOpen(this.PumpenPin))
            {
                var mode = Controller.GetPinMode(this.PumpenPin);
                if (mode != PinMode.Input)
                {
                    Controller.SetPinMode(this.PumpenPin, PinMode.Input);
                    var value = WiringPi.DigitalRead(this.PumpenPin);
                    if (value == 1)
                    {
                        Console.WriteLine("PumpenPin ist immernoch aktiv und Pumpe läuft");
                    }
                    else
                    {
                        Console.WriteLine("Pumpe ausgeschaltet");
                    }
                }
            }
        }

        private void OpenPinAndSetModeToOutput(int pin)
        {
            if (!Controller.IsPinOpen(pin))
            {
                Controller.OpenPin(pin);
                Controller.SetPinMode(pin, System.Device.Gpio.PinMode.Output);
            }
        }

        public bool SetAutomaticModePump(int value)
        {

            // 1. Prüft ob Pumpe bereits läuft
            // Ja => Liefere false (bool) und einen string als Antwort warum Pumpe nicht in den AUtomatic mOdus gesetzt werden kann
            // Nein ==> Mache weiter mit 2.

            //2. Prüfe den Helligkeitssensor
            // Ist Wert > X  ==> HELL ==> Liefere False(bool) und einen string als Antwort ==>  Viel zu Hell zum Gießen
            // Ist Wert < Y ==> DUNKEL ==> PreCondition-Check OK ==>

            //3. Prüfe den Feuchtigkeitssensor
            // Ist Wert X ==> Trocken ==> PreconditionCheck OK ==> Intervall.Lang
            // Ist Wert Y ==> Feucht ==> PreconditionCheck OK ==> Intervall.Kurz
            // Ist Wert Z ==> Nass ==> PreconditionCheck NICHT OK ==> Kein Intervall

            //4 Prüfe die Conditions
            // if(2 == true && 3 == true)
            //{
                this.WasserMarsch(PumpenIntervall.Lang, out var message);
            //}


            return true;
        }
    }
}
