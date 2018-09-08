using log4net;
using log4net.Config;
using RFETIS_2._0.Http;
using RFETIS_2._0.SIL;
using RFETIS_2._0.SIL.Impl;
using RFETIS_2._0.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RFETIS_2._0
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            XmlConfigurator.Configure(new System.IO.FileInfo("../../conf/log4net.xml"));
            InitApp();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));
        private List<EleTag> eleTagList = new List<EleTag>();
        private EleTagSIL sil = new EleTagSILImpl();

        private void InitApp()
        {
            log.Info("Initial Application");

            ServerHelper helper = new ServerHelper(this);
            helper.Setup();

            eleTagList.Add(new EleTag() { Id = 1, Name = "阿斯匹林", Amount = 50 });
            eleTagList.Add(new EleTag() { Id = 2, Name = "康泰克", Amount = 50 });
            eleTagList.Add(new EleTag() { Id = 3, Name = "感冒通", Amount = 50 });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = "RFETIS-2.0";

            // 1. Serial interactive object initial
            sil.setEleTagResponseHandler(EleTagResponseHandler);
            // setPortList(sil.getSerialList());
            // 2. ComboBox initial
            cbPortList.ItemsSource = sil.getSerialList();
            // 3. listView DataContext
            listView.ItemsSource = eleTagList;
        }

        private void updateEleTagState(int guid, string state, EleTagResponseState responseState)
        {
            foreach( EleTag et in eleTagList )
            {
                if( guid == et.Id )
                {
                    et.State = state;
                    et.ResponseState = responseState;
                    break;
                }
            }
        }

        private EleTag getEleTag(int guid)
        {
            foreach (EleTag et in eleTagList)
            {
                if (guid == et.Id)
                {
                    return et;
                }
            }
            return null;
        }

        private void EleTagResponseHandler(int guid, EleTagResponseState state, string msg)
        {
            log.Info("EleTagResponseHandler() -> guid=" + guid + ", state=" + state + ", msg=" + msg);
            switch( state)
            {
                case EleTagResponseState.TAKE_CACHE:
                    updateEleTagState(guid, "加入缓存区(取药)", EleTagResponseState.TAKE_CACHE);
                    break;
                case EleTagResponseState.TAKE_PING:
                    updateEleTagState(guid, "主动查询", EleTagResponseState.TAKE_PING);
                    break;
                case EleTagResponseState.TAKE_SHOW:
                    updateEleTagState(guid, "取药显示", EleTagResponseState.TAKE_SHOW);
                    break;
                case EleTagResponseState.TAKE_SHOW_ERROR:
                    updateEleTagState(guid, "取药显示错误", EleTagResponseState.TAKE_SHOW_ERROR);
                    break;
                case EleTagResponseState.TAKE_TIMEOUT:
                    updateEleTagState(guid, "取药超时", EleTagResponseState.TAKE_TIMEOUT);
                    break;
                case EleTagResponseState.TAKE_ACK:
                    updateEleTagState(guid, "取药确认", EleTagResponseState.TAKE_ACK);
                    break;
                case EleTagResponseState.TAKE_ACK_ERROR:
                    updateEleTagState(guid, "取药确认失败", EleTagResponseState.TAKE_ACK_ERROR);
                    break;
                case EleTagResponseState.TAKE_COMPLETE:
                    updateEleTagState(guid, "取药完成", EleTagResponseState.TAKE_COMPLETE);
                    break;
                case EleTagResponseState.ADD_CACHE:
                    updateEleTagState(guid, "加入缓存区(补药)", EleTagResponseState.ADD_CACHE);
                    break;
                case EleTagResponseState.ADD_PING:
                    updateEleTagState(guid, "主动查询", EleTagResponseState.ADD_PING);
                    break;
                case EleTagResponseState.ADD_SHOW:
                    updateEleTagState(guid, "补药显示", EleTagResponseState.ADD_SHOW);
                    break;
                case EleTagResponseState.ADD_SHOW_ERROR:
                    updateEleTagState(guid, "补药显示错误", EleTagResponseState.ADD_SHOW_ERROR);
                    break;
                case EleTagResponseState.ADD_TIMEOUT:
                    updateEleTagState(guid, "补药超时", EleTagResponseState.ADD_TIMEOUT);
                    break;
                case EleTagResponseState.ADD_ACK:
                    updateEleTagState(guid, "补药确认", EleTagResponseState.ADD_ACK);
                    break;
                case EleTagResponseState.ADD_ACK_ERROR:
                    updateEleTagState(guid, "补药确认失败", EleTagResponseState.ADD_ACK_ERROR);
                    break;
                case EleTagResponseState.ADD_COMPLETE:
                    updateEleTagState(guid, "补药完成", EleTagResponseState.ADD_COMPLETE);
                    break;
            }
        }

        private void btnTakeMedicine_Click(object sender, RoutedEventArgs e)
        {
            log.Info("btnTakeMedicine_Click()");
            int amount = getFromTbAmount();
            if( amount != -1 )
            {
                sil.cacheTakeMedicine(1, amount);
            }
        }

        private void btnAddMedicine_Click(object sender, RoutedEventArgs e)
        {
            log.Info("btnAddMedicine_Click()");
            int amount = getFromTbAmount();
            if (amount != -1)
            {
                sil.cacheAddMedicine(1, amount);
            }
        }

        private void btnOpenSerial_Click(object sender, RoutedEventArgs e)
        {
            log.Info("btnOpenSerial_Click()");
            if( btnOpenSerial.Content.ToString() == "Open" )
            {
                log.Info("btnOpenSerial_Click() -> Open");
                string portName = cbPortList.Text;
                if( portName == "")
                {
                    MessageBox.Show("请先指定串口号!!!");
                    return;
                }
                cbPortList.IsEnabled = false;
                sil.openSerial(portName);
                btnOpenSerial.Content = "Close";
            }
            else if (btnOpenSerial.Content.ToString() == "Close")
            {
                log.Info("btnOpenSerial_Click() -> Close");
                sil.closeSerial();
                cbPortList.IsEnabled = true;
                btnOpenSerial.Content = "Open";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        private void setPortList(List<string> list)
        {
            cbPortList.Items.Clear();
            foreach (string port in list)
            {
                cbPortList.Items.Add(port);
            }
        }

        private int getFromTbAmount()
        {
            string str_amount = tbAmount.Text;
            try
            {
                int i_amount = int.Parse(str_amount);
                return i_amount;
            }
            catch( Exception)
            {
                MessageBox.Show("输入的数量格式错误!!!");
                return -1;
            }
        }

        public bool IsOpen()
        {
            return sil.isSerialOpen();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns>
        /// 0 成功
        /// 1
        /// 2
        /// 3 TAKE_SHOW_ERROR
        /// 4 TAKE_TIMEOUT
        /// 5 OTHER
        /// </returns>
        public int ExecuteTakeMedicine(int id, int amount)
        {
            int result = sil.cacheTakeMedicine(id, amount);
            if( result == 0)
            {
                for (int i = 0; i < 200; i++)
                {
                    EleTag eleTag = getEleTag(id);
                    if (eleTag != null)
                    {
                        EleTagResponseState state = eleTag.ResponseState;
                        if ( state == EleTagResponseState.TAKE_SHOW
                            || state == EleTagResponseState.TAKE_ACK
                            || state == EleTagResponseState.TAKE_COMPLETE
                            || state == EleTagResponseState.TAKE_ACK_ERROR)
                        {
                            return 0;
                        }
                        else if( state == EleTagResponseState.TAKE_SHOW_ERROR )
                        {
                            return 3;
                        }
                        else if( state == EleTagResponseState.TAKE_TIMEOUT )
                        {
                            return 4;
                        }
                        Thread.Sleep(100);
                    }
                }
                return 5;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns>
        /// 0 成功
        /// 1 
        /// 2
        /// 3 ADD_SHOW_ERROR
        /// 4 ADD_TIMEOUT
        /// 5 OTHER
        /// </returns>
        public int ExecuteAddMedicine(int id, int amount)
        {
            int result = sil.cacheAddMedicine(id, amount);
            if (result == 0)
            {
                for (int i = 0; i < 200; i++)
                {
                    EleTag eleTag = getEleTag(id);
                    if (eleTag != null)
                    {
                        EleTagResponseState state = eleTag.ResponseState;
                        if (state == EleTagResponseState.ADD_SHOW
                            || state == EleTagResponseState.ADD_ACK
                            || state == EleTagResponseState.ADD_COMPLETE
                            || state == EleTagResponseState.ADD_ACK_ERROR)
                        {
                            return 0;
                        }
                        else if (state == EleTagResponseState.ADD_SHOW_ERROR)
                        {
                            return 3;
                        }
                        else if (state == EleTagResponseState.ADD_TIMEOUT)
                        {
                            return 4;
                        }
                        Thread.Sleep(100);
                    }
                }
                return 5;
            }
            return result;
        }

        public void InitEleTagList(List<EleTag> list)
        {
            eleTagList.Clear();
            foreach( EleTag e in list )
            {
                eleTagList.Add(new EleTag() { Id=e.Id, Name=e.Name, Amount=e.Amount, ResponseState=EleTagResponseState.NONE, State="" });
            }
            Dispatcher.BeginInvoke(new Action(() => { listView.Items.Refresh(); }));
        }

        public void UpdateEleTagList(List<EleTag> list)
        {
            foreach( EleTag e in list )
            {
                UpdateEleTag(e);
            }
            Dispatcher.BeginInvoke(new Action(() => { listView.Items.Refresh(); }));
        }

        private void UpdateEleTag( EleTag eleTag )
        {
            bool find = false;
            foreach (EleTag e in eleTagList)
            {
                if( e.Id == eleTag.Id )
                {
                    e.Name = eleTag.Name;
                    e.Amount = eleTag.Amount;
                    find = true;
                    break;
                }
            }
            if( !find)
            {
                eleTagList.Add(new EleTag() { Id = eleTag.Id, Name = eleTag.Name, Amount = eleTag.Amount, ResponseState = EleTagResponseState.NONE, State = "" });
            }
        }
    }
}
