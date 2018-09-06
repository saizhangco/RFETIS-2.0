using RFETIS_2._0.HAL;
using RFETIS_2._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.SCL.Impl
{
    class SerialComImpl : SerialCom
    {
        private SerialConfig serialConfig;
        private SerialUtil serialUtil;

        public SerialComImpl()
        {
            serialConfig = new SerialConfig();
            serialUtil = new SerialUtil();
        }

        /// <summary>
        /// 关闭串口
        /// 1 判断串口是否被打开
        /// 2 关闭串口
        /// </summary>
        public void close()
        {
            if (serialUtil.IsOpen())
            {
                serialUtil.close();
            }
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        public int open()
        {
            if (!serialUtil.IsOpen())
            {
                SerialConfig config = new SerialConfig();
                //使用客户端默认配置
                config.PortName = "COM1";
                config.BaudRate = 115200;
                config.DataBits = 8;
                config.Parity = "None";
                config.StopBits = "1";
                return serialUtil.open(config.PortName,
                                config.BaudRate,
                                config.DataBits,
                                config.Parity,
                                config.StopBits);
            }
            return 0;
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        public int open(string portName)
        {
            if (!serialUtil.IsOpen())
            {
                /*
                SerialConfig config = serialConfigDAL.get();
                return serialUtil.open(portName,
                                config.BaudRate,
                                config.DataBits,
                                config.Parity,
                                config.StopBits);
                                */
                return serialUtil.open(portName, 115200, 8, "None", "1");
            }
            return 0;
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public int open(SerialConfig config)
        {
            if (!serialUtil.IsOpen())
            { 
                return serialUtil.open(config.PortName,
                                config.BaudRate,
                                config.DataBits,
                                config.Parity,
                                config.StopBits);
            }
            return 0;
        }

        /// <summary>
        /// 写数据到串口
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool write(byte[] buffer, int offset, int count)
        {
            if (serialUtil.IsOpen())
            {
                serialUtil.write(buffer, offset, count);
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置串口数据接收回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void setSerialDataReceivedHandler(SerialDataReceivedHandler handler)
        {
            serialUtil.SerialDataReceived += handler;
        }
    }
}
