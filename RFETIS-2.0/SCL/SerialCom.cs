using RFETIS_2._0.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.SCL
{
    interface SerialCom
    {
        int open();
        int open(string portName);
        bool write(byte[] data, int start, int length);
        void close();
        void setSerialDataReceivedHandler(SerialDataReceivedHandler handler);
        bool isOpen();
    }
}
