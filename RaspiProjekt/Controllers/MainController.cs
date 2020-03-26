using Microsoft.AspNetCore.Mvc;
using MonitoreCore.Provider.DataProvider;
using System;

namespace MonitoreCore.Controllers
{
    [Route("")]
    [ApiController]
    public class MainController : ControllerBase
    {
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

        [HttpGet("[controller]/[action]")]
        [ActionName("GetChannel")]
        public string GetChannel(int channel)
        {
            DataProvider dataProvider = new DataProvider();
            var value = dataProvider.GetAnalogDataFromSPI(channel);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now + " ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Ausgelesener Wert(Lichtsensor):{value}");
            return value.ToString();
        }



    }
}
