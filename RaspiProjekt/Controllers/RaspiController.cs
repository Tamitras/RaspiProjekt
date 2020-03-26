using Microsoft.AspNetCore.Mvc;
using MonitoreCore.Debug;
using MonitoreCore.Provider.DataProvider;
using System;
using System.Runtime.CompilerServices;

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
    public class RaspiController : ControllerBase
    {
        private RaspiProvider raspiProvider;
        private RaspiProvider RaspiProvider
        {
            get
            {
                return this.raspiProvider;
            }
            set
            {
                raspiProvider = new RaspiProvider();
            }
        }

        private Log Logger => new Log(); 


        /// <summary>
        /// Test Methode zum Testen der Funktionalität
        /// Wird aufgerufen über:
        /// {IpAdress}:{port}/Main/Register
        /// </summary>
        /// <returns></returns>
        [HttpGet("[controller]/[action]")] // Matches '/Main/Register'
        public string Register()
        {
            var text = $"Herzlich Willkommen auf dem Server von Erik";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now + " ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            return text;
        }

        /// <summary>
        /// Test Methode zum Testen der Funktionalität.
        /// {IpAdress}:{port}/Main/Hello?ipAdress={"localhost"}&hostname={"Erik"}");
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

            this.Logger.WriteToConsole(value);

            return value.ToString();
        }
    }
}
