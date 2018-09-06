using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.Model
{
    public class ResponseMessage
    {
        private const byte UART_SOF = 0xFE; //开始字符
        private byte length;                //value字段长度
        private char[] guid = new char[4];  //id
        private char[] command = new char[4];   //命令
        private char[] value = new char[33];                   //值

        public ResponseMessage()
        {
            //初始化
            length = 0;
            for (int i = 0; i < 4; i++)
            {
                guid[i] = '0';
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


        public char[] Command
        {
            get { return command; }
            set { command = value; }
        }

        public char[] Value
        {
            get { return this.value; }
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
                cache[i + 10] = (byte)command[i];
            }
            for (int i = 0; i < length; i++)
            {
                cache[i + 14] = (byte)value[i];
            }
            return cache;
        }
    }
}
