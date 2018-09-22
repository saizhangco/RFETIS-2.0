using RFETIS_2._0.SCL;
using RFETIS_2._0.SCL.Impl;
using RFETIS_2._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Microsoft.Win32;
using System.Threading;
using log4net;

namespace RFETIS_2._0.SIL.Impl
{
    public class RespCache
    {
        public int Guid { get; set; }
        public EleTagResponseState State { get; set; }
    }

    public class EleTagSILImpl : EleTagSIL
    {
        #region 变量定义

        private SerialCom serialCom;
        private EleTagResponseHandler ResponseHandler;
        private RespCache Cache = new RespCache();
        private TaskCachePool taskCachePool = new TaskCachePool();

        private enum Status
        {
            SOF, LENGTH, GUID, COMMAND, VALUE
        };
        private Status status = Status.SOF;
        private ResponseMessage responseMsg = new ResponseMessage();
        private int posi = 0;

        private static readonly ILog log = LogManager.GetLogger(typeof(EleTagSILImpl));

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public EleTagSILImpl()
        {
            serialCom = new SerialComImpl();
            serialCom.setSerialDataReceivedHandler(DataReceivedHandler);
        }

        /// <summary>
        /// 设置串口回调
        /// </summary>
        /// <param name="handler"></param>
        public void setEleTagResponseHandler(EleTagResponseHandler handler)
        {
            ResponseHandler += handler;
            taskCachePool.setEleTagResponseHandler(handler);
        }

        /// <summary>
        /// 取药任务加入缓冲池
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns>
        ///   0  成功
        ///   1  取药任务已经存在
        ///   2  补药任务已经存在
        /// </returns>
        public int cacheTakeMedicine(int id, int amount)
        {
            log.Info("cacheTakeMedicine - id=" + id + ", amount=" + amount);
            int result = taskCachePool.create(TaskCache.TaskCacheType.TAKE, id, amount);
            if( result == 0 )
            {
                ResponseHandler(id, EleTagResponseState.TAKE_CACHE, "");
            }
            return result;
        }

        /// <summary>
        /// 补药任务加入缓冲池
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns>
        ///   0  成功
        ///   1  取药任务已经存在
        ///   2  补药任务已经存在
        /// </returns>
        public int cacheAddMedicine(int id, int amount)
        {
            log.Info("cacheAddMedicine - id=" + id + ", amount=" + amount);
            int result = taskCachePool.create(TaskCache.TaskCacheType.ADD, id, amount);
            if (result == 0)
            {
                ResponseHandler(id, EleTagResponseState.ADD_CACHE, "");
            }
            return result;
        }

        /// <summary>
        /// 获取串口列表
        /// </summary>
        /// <returns></returns>
        public List<string> getSerialList()
        {
            List<string> serialList = new List<string>();
            string[] list = SerialPort.GetPortNames();
            //Initial Serial List
            RegistryKey keyCom = Registry.LocalMachine.OpenSubKey("HARDWARE\\DEVICEMAP\\SERIALCOMM");
            if (keyCom != null)
            {
                string[] sSubKeys = keyCom.GetValueNames();
                foreach (string sName in sSubKeys)
                {
                    string sValue = (string)keyCom.GetValue(sName);
                    serialList.Add(sValue);
                }
                log.Info("getSerialList() -> serialList=[" + string.Join(",",serialList.ToArray()) + "]");
                return serialList;
            }
            else
            {
                log.Info("getSerialList() -> serialList=[" + string.Join(",", serialList.ToArray()) + "]");
                return serialList;
            }
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        public bool openSerial(string portName)
        {
            return serialCom.open(portName) == 0;
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns></returns>
        public bool closeSerial()
        {
            serialCom.close();
            return true;
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        public bool openSerial()
        {
            return serialCom.open() == 0;
        }

        /// <summary>
        /// 判断串口是否已经打开
        /// </summary>
        /// <returns></returns>
        public bool isSerialOpen()
        {
            return serialCom.isOpen();
        }

        /// <summary>
        /// 补药
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="address"></param>
        /// <param name="amount"></param>
        private void AddMedicine(int guid, string address, int amount)
        {
            log.Info("AddMedicine - guid=" + guid + ", address=" + address + ", amount=" + amount);
            RequestMessage requestMessage = new RequestMessage();
            requestMessage.Length = 0;
            requestMessage.Guid = ConvertCom.IntToChar4(guid);
            requestMessage.ShortAddr = ConvertCom.StringToChar4(address);
            // "ADME"
            requestMessage.Command = ConvertCom.StringToChar4("ADME");
            requestMessage.Length = requestMessage.setValue(amount);

            Cache.Guid = 0;
            Cache.State = EleTagResponseState.NONE;
            serialCom.write(requestMessage.getMessageByte(), 0, 14 + requestMessage.Length);
            // 等待成消息返回
            // 最长等待时间10s，循环判断时间间隔10ms
            //int count = 1000;
            //while (count > 0)
            //{
            //    if (Cache.Guid == guid &&
            //        (Cache.State == EleTagResponseState.ADDING
            //        || Cache.State == EleTagResponseState.ADDING_ERROR))
            //    {
            //        count = 0;
            //    }
            //    Thread.Sleep(10);
            //    count--;
            //}
        }

        
        /// <summary>
        /// 取药
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="address"></param>
        /// <param name="amount"></param>
        private void TakeMedicine(int guid, string address, int amount)
        {
            log.Info("TakeMedicine - guid=" + guid + ", address=" + address + ", amount=" + amount);
            RequestMessage requestMessage = new RequestMessage();
            requestMessage.Length = 0;
            requestMessage.Guid = ConvertCom.IntToChar4(guid);
            requestMessage.ShortAddr = ConvertCom.StringToChar4(address);
            // TKME
            requestMessage.Command = ConvertCom.StringToChar4("TKME");
            requestMessage.Length = requestMessage.setValue(amount);

            Cache.Guid = 0;
            Cache.State = EleTagResponseState.NONE;
            serialCom.write(requestMessage.getMessageByte(), 0, 14 + requestMessage.Length);
            // 最长等待时间10s，循环判断时间间隔10ms
            //int count = 1000;
            //while (count > 0)
            //{
            //    if (Cache.Guid == guid &&
            //        (Cache.State == EleTagResponseState.TAKING
            //        || Cache.State == EleTagResponseState.TAKING_ERROR))
            //    {
            //        count = 0;
            //    }
            //    Thread.Sleep(10);
            //    count--;
            //}
        }

        /// <summary>
        /// 取药确认
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="address"></param>
        private void ExecuteTakeAck(int guid, string address)
        {
            log.Info("ExecuteTakeAck - guid=" + guid + ", address=" + address + " ,");
            //1 获取通信地址 -> 验证通信地址
            RequestMessage requestMessage = new RequestMessage();
            requestMessage.Length = 2;
            requestMessage.Guid = ConvertCom.IntToChar4(guid);
            requestMessage.ShortAddr = ConvertCom.StringToChar4(address);
            // AKTK
            requestMessage.Command = ConvertCom.StringToChar4("AKTK");
            requestMessage.Value[0] = 'O';
            requestMessage.Value[1] = 'K';

            serialCom.write(requestMessage.getMessageByte(), 0, 14 + requestMessage.Length);
        }

        /// <summary>
        /// 补药确认
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="address"></param>
        private void ExecuteAddAck(int guid, string address)
        {
            log.Info("ExecuteAddAck - guid=" + guid + ", address=" + address.ToString() + " ,");
            //1 获取通信地址 -> 验证通信地址
            RequestMessage requestMessage = new RequestMessage();
            requestMessage.Length = 2;
            requestMessage.Guid = ConvertCom.IntToChar4(guid);
            requestMessage.ShortAddr = ConvertCom.StringToChar4(address);
            // AKAD
            requestMessage.Command = ConvertCom.StringToChar4("AKAD");
            requestMessage.Value[0] = 'O';
            requestMessage.Value[1] = 'K';

            serialCom.write(requestMessage.getMessageByte(), 0, 14 + requestMessage.Length);
        }

        /// <summary>
        /// 查询确认
        /// </summary>
        /// <param name="guid"></param>
        private void ExecuteQueryAck(int guid, string address, int residualQuantity)
        {
            log.Info("ExecuteQueryAck - guid=" + guid + ", address=" + address + ", residualQuantity=" + residualQuantity);
            //1 获取通信地址
            //2 获取药品剩余量

            RequestMessage requestMessage = new RequestMessage();
            requestMessage.Length = 0;
            requestMessage.Guid = ConvertCom.IntToChar4(guid);
            requestMessage.ShortAddr = ConvertCom.StringToChar4(address);
            // LTME
            requestMessage.Command = ConvertCom.StringToChar4("LTME");
            requestMessage.Length = requestMessage.setValue(residualQuantity);

            serialCom.write(requestMessage.getMessageByte(), 0, 14 + requestMessage.Length);
        }

        /// <summary>
        /// 串口通信异步回调
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        private void DataReceivedHandler(byte[] buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer[i] < 32 && buffer[i] != 10 && buffer[i] != 13)
                {
                    //SetRtbRxConsole("[" + (int)data[i] + "]");
                }
                else
                {
                    //SetRtbRxConsole(data[i].ToString());
                }
                switch (status)
                {
                    case Status.SOF:
                        if (buffer[i] == '#')
                        {
                            status = Status.LENGTH;
                        }
                        break;
                    case Status.LENGTH:
                        responseMsg.Length = buffer[i];
                        status = Status.GUID;
                        break;
                    case Status.GUID:
                        responseMsg.Guid[posi++] = (char)buffer[i];
                        if (posi >= 4)
                        {
                            posi = 0;
                            status = Status.COMMAND;
                        }
                        break;
                    case Status.COMMAND:
                        responseMsg.Command[posi++] = (char)buffer[i];
                        if (posi >= 4)
                        {
                            posi = 0;
                            if (responseMsg.Length == 0)
                            {
                                status = Status.SOF;
                                // 处理ResponseMsg
                                ExecuteResponseMsg();
                            }
                            else
                            {
                                status = Status.VALUE;
                            }
                        }
                        break;
                    case Status.VALUE:
                        responseMsg.Value[posi++] = (char)buffer[i];
                        if (posi >= responseMsg.Length || posi >= 33)
                        {
                            responseMsg.Value[posi] = '\0'; //字符串结束，方便后面使用
                            posi = 0;
                            status = Status.SOF;
                            // 处理ResponseMsg
                            ExecuteResponseMsg();
                        }
                        break;
                    default:
                        status = Status.SOF;
                        break;
                }
            }
        }

        /// <summary>
        /// 电子标签返回消息处理
        /// </summary>
        private void ExecuteResponseMsg()
        {
            //SetRtbRxConsole(responseMsg.getMessageByte);
            int id = ConvertCom.Char4ToInt(responseMsg.Guid);
            string command = new string(responseMsg.Command);
            // 1层数据流
            if (command == "PING")
            {
                //在界面上显示心跳
                string _shortAddr = new string(responseMsg.Value, 0, 4);
                //rftaglist[id - Offset - 1].heartBeat(8);
                //SetRtbStatusConsole("" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 网络地址[" + _shortAddr + "]\n");
                //SessionArray[id].ShortAddr = _shortAddr;
                //rftaglist[id - Offset - 1].SetLabelTagID(_shortAddr);
                /*
                 * 1 从数据库中获取guid的mapping信息。
                 * 2 信息存在，判断address是否一致，不一致则更新。
                 * 3 信息不存在，创建一个新的mapping，并保存在数据库。
                 * 
                 * 1 通过id从缓冲池中获取取药或补药信息
                 * 2 
                 */
                // ResponseHandler(id, EleTagResponseState.ADDRESS, _shortAddr);

                TaskCache cache = taskCachePool.getTaskCache(id);
                if( cache != null )
                {
                    log.Info("ExecuteResponseMsg - PING - cache exist, id=" + id + ", type=" + cache.Type);
                    switch ( cache.Type )
                    {
                        case TaskCache.TaskCacheType.TAKE:
                            TakeMedicine(id, _shortAddr, cache.Amount);
                            ResponseHandler(id, EleTagResponseState.TAKE_PING, _shortAddr);
                            break;
                        case TaskCache.TaskCacheType.ADD:
                            AddMedicine(id, _shortAddr, cache.Amount);
                            ResponseHandler(id, EleTagResponseState.ADD_PING, _shortAddr);
                            break;
                    }
                }
                else
                {
                    log.Info("ExecuteResponseMsg - PING - cache is null, id=" + id);
                }

            }
            // 2层数据流
            else if (command == "TKME")
            {
                taskCachePool.destory(id);
                log.Info("ExecuteResponseMsg - TKME - cache destory, id=" + id);
                string respResult = new string(responseMsg.Value, 0, responseMsg.Length);
                if (respResult == "OK")
                {
                    //rftaglist[id - Offset - 1].darkenLED(2);
                    //rftaglist[id - Offset - 1].lightLED(1);
                    //SetRtbStatusConsole("" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 取药命令[成功]\n");
                    ResponseHandler(id, EleTagResponseState.TAKE_SHOW, "");
                    Cache.Guid = id;
                    Cache.State = EleTagResponseState.TAKE_SHOW;
                }
                else
                {
                    //rftaglist[id - Offset - 1].darkenLED(1);
                    //rftaglist[id - Offset - 1].darkenLED(2);
                    //string alertValue = "" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 补药命令[错误:" + respResult + "]\n";
                    //SetRtbStatusConsole(alertValue);
                    //MessageBox.Show(alertValue);
                    ResponseHandler(id, EleTagResponseState.TAKE_SHOW_ERROR, respResult);
                    Cache.Guid = id;
                    Cache.State = EleTagResponseState.TAKE_SHOW_ERROR;
                }
            }
            // 2层数据流
            else if (command == "ADME")
            {
                taskCachePool.destory(id);
                log.Info("ExecuteResponseMsg - ADME - cache destory, id=" + id);
                string respResult = new string(responseMsg.Value, 0, responseMsg.Length);
                if (respResult == "OK")
                {
                    //rftaglist[id - Offset - 1].darkenLED(1);
                    //rftaglist[id - Offset - 1].lightLED(2);
                    //SetRtbStatusConsole("" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 补药命令[成功]\n");
                    ResponseHandler(id, EleTagResponseState.ADD_SHOW, "");
                    Cache.Guid = id;
                    Cache.State = EleTagResponseState.ADD_SHOW;
                }
                else
                {
                    //rftaglist[id - Offset - 1].darkenLED(1);
                    //rftaglist[id - Offset - 1].darkenLED(2);
                    //string alertValue = "" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 补药命令[错误:" + respResult + "]\n";
                    //SetRtbStatusConsole(alertValue);
                    //MessageBox.Show(alertValue);
                    ResponseHandler(id, EleTagResponseState.ADD_SHOW_ERROR, respResult);
                    Cache.Guid = id;
                    Cache.State = EleTagResponseState.ADD_SHOW_ERROR;
                }
            }
            // 3层数据流
            else if (command == "AKTK")
            {
                //1 判断是否为Push Button首次确认
                if (responseMsg.Length == 6 || new string(responseMsg.Value, 0, 2)=="AD")
                {
                    string _shortAddr = new string(responseMsg.Value, 2, 4);
                    ResponseHandler(id, EleTagResponseState.TAKE_ACK, "");
                    ExecuteTakeAck(id, _shortAddr);
                }
                // 2 
                else
                {
                    string respResult = new string(responseMsg.Value, 0, responseMsg.Length);
                    if (respResult == "OK")
                    {
                        //rftaglist[id - Offset - 1].darkenLED(1);
                        //SetRtbStatusConsole("" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 取药确认[成功]\n");
                        ResponseHandler(id, EleTagResponseState.TAKE_COMPLETE, "");
                    }
                    else
                    {
                        log.Info(respResult);
                        //rftaglist[id - Offset - 1].darkenLED(1);
                        //string alertValue = "" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 取药确认[错误:" + respResult + "]\n";
                        //SetRtbStatusConsole(alertValue);
                        //MessageBox.Show(alertValue);
                        ResponseHandler(id, EleTagResponseState.TAKE_ACK_ERROR, respResult);
                    }
                }
            }
            // 3层数据流
            else if (command == "AKAD")
            {
                //1 判断是否为Push Button首次确认
                if (responseMsg.Length == 6 || new string(responseMsg.Value, 0, 2) == "AD")
                {
                    string _shortAddr = new string(responseMsg.Value, 2, 4);
                    ResponseHandler(id, EleTagResponseState.ADD_ACK, "");
                    ExecuteAddAck(id, _shortAddr);
                }
                // 2 
                else
                {
                    string respResult = new string(responseMsg.Value, 0, responseMsg.Length);
                    if (respResult == "OK")
                    {
                        //rftaglist[id - Offset - 1].darkenLED(2);
                        //SetRtbStatusConsole("" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 补药确认[成功]\n");
                        ResponseHandler(id, EleTagResponseState.ADD_COMPLETE, "");
                    }
                    else
                    {
                        //rftaglist[id - Offset - 1].darkenLED(2);
                        //string alertValue = "" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 补药确认[错误:" + respResult + "]\n";
                        //SetRtbStatusConsole(alertValue);
                        //MessageBox.Show(alertValue);
                        ResponseHandler(id, EleTagResponseState.ADD_ACK_ERROR, respResult);
                    }
                }
            }
            // 3层数据流
            else if (command == "LTME")
            {
                //1 判断是否为Push Button首次确认
                if (responseMsg.Length == 6 || new string(responseMsg.Value, 0, 2) == "AD")
                {
                    string _shortAddr = new string(responseMsg.Value, 2, 4);
                    ResponseHandler(id, EleTagResponseState.NONE, "");
                    ExecuteQueryAck(id, _shortAddr, new Random().Next(1,11));// [1,11)
                }
                // 2 
                else
                {
                    string respResult = new string(responseMsg.Value, 0, responseMsg.Length);
                    if (respResult == "OK")
                    {
                        //SetRtbStatusConsole("" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 查询确认[成功]\n");
                    }
                    else
                    {
                        //string alertValue = "" + GenericUtil.Generic_ConvertToGuid(id) + " ----> 查询确认[错误:" + respResult + "]\n";
                        //SetRtbStatusConsole(alertValue);
                        //MessageBox.Show(alertValue);
                    }
                }
            }
        }
    }
}
