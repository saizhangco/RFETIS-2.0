using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.Model
{
    class SerialConfig
    {
        private string portName;
        private int baudRate;
        private int dataBits;
        private string parity;
        private string stopBits;

        public string PortName
        {
            get { return portName; }
            set { portName = value; }
        }

        public int BaudRate
        {
            get { return baudRate; }
            set { baudRate = value; }
        }

        public int DataBits
        {
            get { return dataBits; }
            set { dataBits = value; }
        }

        public string Parity
        {
            get { return parity; }
            set { parity = value; }
        }

        public string StopBits
        {
            get { return stopBits; }
            set { stopBits = value; }
        }
    }
}
