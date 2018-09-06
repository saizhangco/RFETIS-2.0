using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.HAL
{
    class SerialUtil
    {
        private SerialPort comm;
        private bool isInit = false; //是否已经完成初始化
        public SerialDataReceivedHandler SerialDataReceived;

        public SerialUtil()
        {
            comm = new SerialPort(); //串口对象
            comm.DataReceived += Comm_DataReceived;
        }

        /// <summary>
        /// 串口操作对象的初始化
        /// </summary>
        /// <param name="portName">串口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="parityBit">校验位</param>
        /// <param name="stopBits">停止位</param>
        /// <returns>
        ///     0 初始化成功
        ///     1 校验位设置异常
        ///     2 停止位设置异常
        ///     3 打开串口异常
        /// </returns>
        public int open(string portName, int baudRate, int dataBits, string parityBit, string stopBits)
        {
            comm.PortName = portName;   //串口号
            comm.BaudRate = baudRate;   //波特率
            comm.DataBits = dataBits;   //数据位
            if (parityBit == "None")     //校验位
            {
                comm.Parity = Parity.None;
            }
            else if (parityBit == "Even")
            {
                comm.Parity = Parity.Even;
            }
            else if (parityBit == "Odd")
            {
                comm.Parity = Parity.Odd;
            }
            else if (parityBit == "Mark")
            {
                comm.Parity = Parity.Mark;
            }
            else if (parityBit == "Space")
            {
                comm.Parity = Parity.Space;
            }
            else
            {
                //Exception
                return 1;
            }

            if (stopBits == "None")     //停止位
            {
                comm.StopBits = StopBits.None;
            }
            else if (stopBits == "1")
            {
                comm.StopBits = StopBits.One;
            }
            else if (stopBits == "1.5")
            {
                comm.StopBits = StopBits.OnePointFive;
            }
            else if (stopBits == "2")
            {
                comm.StopBits = StopBits.Two;
            }
            else
            {
                //Exception
                return 2;
            }
            comm.ReadBufferSize = 1024;     //设置接受缓冲区大小
            comm.WriteBufferSize = 1024;    //设置发送缓冲区大小
            try
            {
                comm.Open();    //打开串口
            }
            catch (Exception)
            {
                //捕获到异常信息，创建一个新的comm对象，之前的不能用了
                comm = new SerialPort();
                Console.WriteLine("Error : Can't open the serial port!");
                return 3;
            }
            isInit = true;
            return 0;
        }

        /// <summary>
        /// 写入串口数据函数
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>
        ///     0 成功写入串口数据
        ///     1 串口未打开
        ///     2 串口对象未完成初始化
        /// </returns>
        public int write(byte[] buffer, int offset, int count)
        {
            if (!isInit)
            {
                Console.WriteLine("串口对象未完成初始化，或初始化存在异常");
                return 2;
            }
            if (!comm.IsOpen)  //判断串口是否已经打开
            {
                Console.WriteLine(comm.PortName + "串口未打开，无法执行写入操作！");
                return 1;
            }
            comm.Write(buffer, offset, count);
            return 0;
        }


        public void close()
        {
            //判断串口对象是否为空
            if (comm != null)
            {
                comm.Close();
            }
            comm = null;
        }

        /// <summary>
        /// 获取当前串口状态
        /// </summary>
        /// <returns></returns>
        public bool IsOpen()
        {

            return comm.IsOpen;
        }

        private void Comm_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int count = comm.BytesToRead;
            if (count > 0)
            {
                byte[] buffer = new byte[count];
                comm.Read(buffer, 0, count);
                SerialDataReceived(buffer, count);
            }
        }
    }
}
