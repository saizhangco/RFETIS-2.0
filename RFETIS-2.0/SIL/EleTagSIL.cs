using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.SIL
{
    interface EleTagSIL
    {
        // int takeMedicine(int guid, string address, int amount);
        // int addMedicine(int guid, string address, int amount);
        void setEleTagResponseHandler(EleTagResponseHandler handler);
        int cacheTakeMedicine(int id, int amount);
        int cacheAddMedicine(int id, int amount);
        List<string> getSerialList();
        bool openSerial(string portName);
        bool openSerial();
        bool closeSerial();
        bool isSerialOpen();
    }
}
