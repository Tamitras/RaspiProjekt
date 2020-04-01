using System;
using System.Collections.Generic;
using System.Text;

namespace MonitoreCore.Interfaces
{
    public interface IRaspiProvider
    {
        /// <summary>
        /// Liefert den analogen Wert des Mcp auf einem Channel
        /// </summary>
        /// <param name="channel">Analoger Eingang des MCPs</param>
        /// <returns>Wert</returns>
        int GetAnalogDataFromSPI(int channel);

        /// <summary>
        /// Noch nicht implementiert
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        bool GetDigitalData(int pin);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool WriteDigitalData(int pin, int value);

        /// <summary>
        /// Steuert die Pumpe an und lässt das Gießen beginnen.
        /// </summary>
        /// <param name="intervall">Länge der Bewässerung</param>
        /// <returns>Zeitsperre bis zum nächsten Bewässern</returns>
        void WasserMarsch(int intervall, out string message);
    }
}
