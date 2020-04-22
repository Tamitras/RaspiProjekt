using Microsoft.AspNetCore.Mvc;
using MonitoreCore.Debug;
using MonitoreCore.Enum;
using MonitoreCore.Provider.DataProvider;
using System;

namespace MonitoreCore.Controllers
{
    /// <summary>
    /// .Net Core Web-Controller 
    /// Stellt Methoden bereit, die über eine Rest-Schnitstelle aufgerufen werden können
    /// Z.B.
    /// App (Android)
    /// Webseite
    /// </summary>
    [Route("")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private RaspiProvider RaspiProvider { get; set; } = new RaspiProvider();
        private Log Logger { get; set; } = new Log();

        private string GetAppVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Test Methode zum Testen der Funktionalität
        /// Wird aufgerufen über:
        /// {IpAdress}:{port}/Main/Register
        /// </summary>
        /// <returns></returns>
        [HttpGet("[controller]/[action]")] // Matches '/Main/Register'
        public string Register()
        {
            var text = $"Bewässerungs-App betriebsbereit v.{this.GetAppVersion()}";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now + " ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);

            this.Logger.Add(LogType.Info, text);

            return text;
        }

        /// <summary>
        /// Test Methode zum Testen der Funktionalität.
        /// {IpAdress}:{port}/Main/Hello?ipAdress={"192.168."}&hostname={"Erik"}");
        /// </summary>
        /// <param name="ipAdress"></param>
        /// <param name="hostname"></param>
        /// <returns></returns>
        [HttpGet("[controller]/[action]")]
        [ActionName("Hello")]
        public string Hello(string ipAdress, string hostname)
        {
            var text = $"[{ipAdress}]({hostname}) says Hello";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now + " ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            return text;
        }

        /// <summary>
        /// Liefert den analogen Wert des MCP Modules eines gewissen Channels
        /// Channel 1-8 (0-7)
        /// {IpAdress}:{port}/Main/GetChannel?Channel={"0"}");
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        [HttpGet("[controller]/[action]")]
        [ActionName("GetChannel")]
        public string GetChannel(int channel)
        {
            var value = this.RaspiProvider.GetAnalogDataFromSPI(channel);

            this.Logger.WriteToConsole("Ausgelesener Wert(LichtSensor): ", value);
            this.Logger.Add(LogType.Info, $"Channel {channel} wurde ausgelesen. Wert: {value}");

            return value.ToString();

        }

        [HttpGet("[controller]/[action]")]
        [ActionName("SetGPIO")]
        public bool SetGPIO(int pin, int value)
        {
            var ret = default(bool);

            try
            {
                ret = this.RaspiProvider.WriteDigitalData(pin, value);
            }
            catch (Exception ex)
            {
                this.Logger.WriteToConsole($"Led-Status fehlerhaft");
                Console.WriteLine($"Fehler: {ex}");
            }
            finally
            {
                this.Logger.WriteToConsole($"Led-Status Pin: {pin}", value);
                this.Logger.Add(Enum.LogType.Info, $"Gpio Pin {pin} wurde angesprochen. Wert: {value}");
            }

            return ret;
        }

        [HttpGet("[controller]/[action]")]
        [ActionName("SetAutomaticModePump")]
        public bool SetAutomaticModePump(int value)
        {
            var ret = default(bool);

            try
            {
                //ret = this.RaspiProvider.WriteDigitalData(pin, value);
            }
            catch (Exception ex)
            {
                this.Logger.WriteToConsole($"Fehler: {ex.Message}");
            }

            return ret;
        }
    }
}
