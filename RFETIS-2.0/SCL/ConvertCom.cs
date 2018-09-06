using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.SCL
{
    class ConvertCom
    {
        public static char[] IntToChar4(int value)
        {
            char[] ca = new char[4];
            int tmp = value;
            for (int i = 4; i > 0; i--)
            {
                ca[i - 1] = IntToCharHex(tmp % 16);
                tmp /= 16;
            }
            return ca;
        }

        public static char IntToCharHex(int value)
        {
            return (char)(value > 9 ? (value - 10 + 'A') : value + '0');
        }

        public static char[] StringToChar4(string value)
        {
            char[] ca = new char[4];
            char[] tmp = value.ToCharArray();
            for (int i = 0; i < 4; i++)
            {
                ca[i] = tmp[i];
            }
            return ca;
        }

        public static int Char4ToInt(char[] ca)
        {
            int retValue = 0;
            for (int i = 0; i < 4; i++)
            {
                retValue *= 16;
                retValue += CharHexToInt(ca[i]);
            }
            return retValue;
        }

        public static int CharHexToInt(char c)
        {
            return c >= 'A' ? (c - 'A' + 10) : (c - '0');
        }
    }
}
