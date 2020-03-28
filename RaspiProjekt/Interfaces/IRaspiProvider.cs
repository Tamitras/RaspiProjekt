using System;
using System.Collections.Generic;
using System.Text;

namespace MonitoreCore.Interfaces
{
    interface IRaspiProvider
    {
        int GetAnalogDataFromSPI(int channel, out string exception);

        bool GetDigitalData(int pin);

        bool WriteDititalData(int pin, int value);
    }
}
