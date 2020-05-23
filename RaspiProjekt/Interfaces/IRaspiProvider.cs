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
        /// 
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool WriteDigitalData(int pin, int value);

        /// <summary>
        /// Setzt den Automatischen Modus für die Pumpe
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetAutoMode(int value);
    }
}
