using System;
using System.Collections.Generic;
using System.Text;

namespace MonitoreCore.Interfaces
{
    interface IDataProvider
    {
        int GetAnalogDataFromSPI(int channel);

        bool GetDigitalData(int pin);

        void WriteDititalData(int pin, bool value);
    }
}
