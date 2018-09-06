using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.Model
{
    public class RequestMessage
    {
        private const byte UART_SOF = (byte)'#'; //开始字符
        private byte length;                //value字段长度
        private char[] guid = new char[4];  //id
        private char[] shortAddr = new char[4]; //shortAddress
        private char[] command = new char[4];   //命令
        private char[] value = new char[32];  //值

        public RequestMessage()
        {
            //初始化
            length = 0;
            for (int i = 0; i < 4; i++)
            {
                guid[i] = '0';
            }
            for (int i = 0; i < 4; i++)
            {
                shortAddr[i] = '0';
            }
            for (int i = 0; i < 4; i++)
            {
                command[i] = '0';
            }
        }

        public byte Length
        {
            get { return length; }
            set { length = value; }
        }

        public char[] Guid
        {
            get { return guid; }
            set { guid = value; }
        }

        public char[] ShortAddr
        {
            get { return shortAddr; }
            set { shortAddr = value; }
        }

        public char[] Command
        {
            get { return command; }
            set { command = value; }
        }

        public char[] Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public byte[] getMessageByte()
        {
            byte[] cache = new byte[length + 14];
            cache[0] = UART_SOF;
            cache[1] = length;
            for (int i = 0; i < 4; i++)
            {
                cache[i + 2] = (byte)guid[i];
            }
            for (int i = 0; i < 4; i++)
            {
                cache[i + 6] = (byte)shortAddr[i];
            }
            for (int i = 0; i < 4; i++)
            {
                cache[i + 10] = (byte)command[i];
            }
            for (int i = 0; i < length; i++)
            {
                cache[i + 14] = (byte)value[i];
            }
            return cache;
        }

        public string getMessageString()
        {
            string retValue = "";
            retValue += "" + (char)UART_SOF;
            retValue += "" + length;
            for (int i = 0; i < 4; i++)
            {
                retValue += "" + guid[i];
            }
            for (int i = 0; i < 4; i++)
            {
                retValue += "" + shortAddr[i];
            }
            for (int i = 0; i < 4; i++)
            {
                retValue += "" + command[i];
            }
            for (int i = 0; i < length; i++)
            {
                retValue += "" + value[i];
            }
            return retValue;
        }

        public byte setValue(int value)
        {
            // 将int类型的数据转换成十进制字符数据格式，并返回长度
            int tmp = value;
            byte posi = 0;
            while (tmp > 0 && posi < 32)
            {
                this.value[posi++] = (char)(tmp % 10 + '0');
                tmp /= 10;
            }
            for (int i = 0; i < posi / 2; i++)
            {
                char t = this.value[i];
                this.value[i] = this.value[posi - i - 1];
                this.value[posi - i - 1] = t;
            }
            return posi;
        }
    }
}
