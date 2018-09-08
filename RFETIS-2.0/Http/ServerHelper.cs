using log4net;
using RFETIS_2._0.Utils;
using RFETIS_2._0.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.Http
{
    public class ServerHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ServerHelper));
        private MainWindow parent;
        public ServerHelper(MainWindow parent)
        {
            this.parent = parent;
        }
        HttpListener httpListener = new HttpListener();
        public void Setup(int port = 8089)
        {
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            httpListener.Prefixes.Add(string.Format("http://*:8089/", port));
            // 如果发送到8080端口没有被处理，则这里全部受理，+是全部接收
            httpListener.Start();   //开启服务

            Receive();  //异步接收请求
        }

        private void Receive()
        {
            httpListener.BeginGetContext(new AsyncCallback(EndReceive), null);
        }

        void EndReceive(IAsyncResult ar)
        {
            var context = httpListener.EndGetContext(ar);
            Dispather(context); //解析请求
            Receive();
        }

        RequestHelper RequestHelper;
        ResponseHelper ResponseHelper;
        void Dispather(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            RequestHelper = new RequestHelper(request);
            ResponseHelper = new ResponseHelper(response);
            //RequestHelper.DispatchResources(fs =>
            //{
            //    ResponseHelper.WriteToClient(fs);   //对相应的请求做出回应
            //});
            RequestHelper.DispatchActions(httpRequest => {
                if (httpRequest.Url == "/task" && httpRequest.Method == "POST")
                {
                    if (httpRequest.EntityBody == null)
                    {
                        context.Response.StatusCode = 500;
                        ResponseHelper.ResponseToClient();
                    }
                    else
                    {
                        log.Info("Dispather() -> info=" + httpRequest.EntityBody.ToString());
                        //判断串口是否已经打开
                        log.Info("Dispather() -> IsOpen=" + parent.IsOpen());
                        context.Response.StatusCode = 200;
                        if ( !parent.IsOpen() )
                        {
                            ResponseHelper.ResponseToClient(ExceptionJson("Serial_Not_Open"));
                        }
                        else
                        {
                            if( httpRequest.EntityBody.Type == "TAKE")
                            {
                                int result = parent.ExecuteTakeMedicine(httpRequest.EntityBody.Guid, httpRequest.EntityBody.Value);
                                switch(result)
                                {
                                    case 0:
                                        ResponseHelper.ResponseToClient(ResultJson("Successful"));
                                        break;
                                    case 1:
                                        ResponseHelper.ResponseToClient(ResultJson("Take_Medicine_Task_Exist"));
                                        break;
                                    case 2:
                                        ResponseHelper.ResponseToClient(ResultJson("Add_Medicine_Task_Exist"));
                                        break;
                                    case 3:
                                        ResponseHelper.ResponseToClient(ResultJson("Take_Show_Error"));
                                        break;
                                    case 4:
                                        ResponseHelper.ResponseToClient(ResultJson("Take_Timeout"));
                                        break;
                                    case 5:
                                        ResponseHelper.ResponseToClient(ResultJson("System_Timeout"));
                                        break;
                                    default:
                                        ResponseHelper.ResponseToClient(ResultJson("Other_Error"));
                                        break;
                                }
                            }
                            else if( httpRequest.EntityBody.Type == "ADD" )
                            {
                                int result = parent.ExecuteAddMedicine(httpRequest.EntityBody.Guid, httpRequest.EntityBody.Value);
                                switch (result)
                                {
                                    case 0:
                                        ResponseHelper.ResponseToClient(ResultJson("Successful"));
                                        break;
                                    case 1:
                                        ResponseHelper.ResponseToClient(ResultJson("Take_Medicine_Task_Exist"));
                                        break;
                                    case 2:
                                        ResponseHelper.ResponseToClient(ResultJson("Add_Medicine_Task_Exist"));
                                        break;
                                    case 3:
                                        ResponseHelper.ResponseToClient(ResultJson("Add_Show_Error"));
                                        break;
                                    case 4:
                                        ResponseHelper.ResponseToClient(ResultJson("Add_Timeout"));
                                        break;
                                    case 5:
                                        ResponseHelper.ResponseToClient(ResultJson("System_Timeout"));
                                        break;
                                    default:
                                        ResponseHelper.ResponseToClient(ResultJson("Other_Error"));
                                        break;
                                }
                            }
                            else
                            {
                                ResponseHelper.ResponseToClient(ExceptionJson("Type_Error"));
                            }
                            
                        }
                    }
                }
                else if (httpRequest.Url == "/data" && httpRequest.Method == "POST")
                {
                    if (httpRequest.ListBody == null)
                    {
                        context.Response.StatusCode = 500;
                        ResponseHelper.ResponseToClient();
                    }
                    else
                    {
                        log.Info("Dispather() -> data=" + ArrayUtils<EleTag>.ToString(httpRequest.ListBody));
                        context.Response.StatusCode = 200;
                        ResponseHelper.ResponseToClient(ResultJson("POST " + httpRequest.ListBody.Count + " -> " + ArrayUtils<EleTag>.ToString(httpRequest.ListBody)));
                    }
                }
                else if (httpRequest.Url == "/data" && httpRequest.Method == "PUT")
                {
                    if (httpRequest.ListBody == null)
                    {
                        context.Response.StatusCode = 500;
                        ResponseHelper.ResponseToClient();
                    }
                    else
                    {
                        log.Info("Dispather() -> data=" + ArrayUtils<EleTag>.ToString(httpRequest.ListBody));
                        context.Response.StatusCode = 200;
                        ResponseHelper.ResponseToClient(ResultJson("PUT " + +httpRequest.ListBody.Count + " -> " + ArrayUtils<EleTag>.ToString(httpRequest.ListBody)));
                    }
                }
                else if(httpRequest.Method == "GET")
                {
                    string filePath = string.Format(@"{0}/../../wwwroot{1}", Environment.CurrentDirectory, httpRequest.Url);
                    if (httpRequest.Url.Length == 1)
                    {
                        filePath = string.Format(@"{0}/../../wwwroot/index.html", Environment.CurrentDirectory);  //默认访问文件
                    }
                    if (File.Exists(filePath))
                    {
                        context.Response.StatusCode = 200;
                        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                        ResponseHelper.WriteToClient(fs);
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        ResponseHelper.ResponseToClient();
                    }
                }
                else
                {
                    context.Response.StatusCode = 404;
                    ResponseHelper.ResponseToClient();
                }
            });
        }

        private string ResultJson(string value)
        {
            return "{ \"Result\": \"" + value + "\" }";
        }

        private string ExceptionJson(string value)
        {
            return "{ \"Exception\": \"" + value + "\" }";
        }
    }
}
