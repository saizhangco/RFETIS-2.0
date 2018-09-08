using Newtonsoft.Json;
using RFETIS_2._0.Model;
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
    public class RequestHelper
    {
        private HttpListenerRequest request;
        public RequestHelper(HttpListenerRequest request)
        {
            this.request = request;
        }
        public Stream RequestStream { get; set; }
        public void ExtracHeader()
        {
            RequestStream = request.InputStream;
        }

        public delegate void ExecutingDispatch(FileStream fs);
        public void DispatchResources(ExecutingDispatch action)
        {
            var rawUrl = request.RawUrl;    //资源默认放在执行程序的wwwroot文件夹下，默认文档为index.html
            string filePath = string.Format(@"{0}/wwwroot{1}", Environment.CurrentDirectory, rawUrl);
            Console.WriteLine("DispatchResources() : filePath=[" + filePath + "]"); //这里对应请求其他类型资源，如图片，文本等
            if (rawUrl.Length == 1)
            {
                filePath = string.Format(@"{0}/wwwroot/index.html", Environment.CurrentDirectory);  //默认访问文件
                Console.WriteLine("DispatchResources() : filePath={" + filePath + "}");
            }
            try
            {
                if (File.Exists(filePath))
                {
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                    action?.Invoke(fs);
                }
            }
            catch
            {
                return;
            }
        }

        public delegate void ExecutingDispatchActions(HttpRequest<EletagInfo, EleTag> httpRequest);
        public void DispatchActions(ExecutingDispatchActions action)
        {
            var rawUrl = request.RawUrl;
            var method = request.HttpMethod;
            Console.WriteLine("DispatchActions() : rawUrl=[" + rawUrl + "]");
            Console.WriteLine("DispatchActions() : method=[" + method + "]");
            HttpRequest<EletagInfo, EleTag> httpRequest = new HttpRequest<EletagInfo, EleTag>();
            httpRequest.Method = method;
            httpRequest.Url = rawUrl;

            string data = RequestEntityBody();
            if (data != null)
            {
                if (httpRequest.Url == "/task")
                {
                    // 解析data
                    EletagInfo info = JsonConvert.DeserializeObject<EletagInfo>(data);
                    httpRequest.EntityBody = info;
                    httpRequest.ListBody = null;
                }
                else if(httpRequest.Url == "/data")
                {
                    // 解析data
                    List<EleTag> list = JsonConvert.DeserializeObject<List<EleTag>>(data);
                    httpRequest.ListBody = list;
                    httpRequest.EntityBody = null;
                }
            }
            else
            {
                httpRequest.EntityBody = null;
                httpRequest.ListBody = null;
            }
            action?.Invoke(httpRequest);
        }

        private string RequestEntityBody()
        {
            if (!request.HasEntityBody)
            {
                return null;
            }
            using (Stream body = request.InputStream) // here we have data
            {
                using (StreamReader reader = new StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public void ResponseQuerys()
        {
            var querys = request.QueryString;
            foreach (string key in querys.AllKeys)
            {
                VarityQuerys(key, querys[key]);
            }
        }

        private void VarityQuerys(string key, string value)
        {
            switch (key)
            {
                case "pic": Pictures(value); break;
                case "text": Texts(value); break;
                default: Defaults(value); break;
            }
        }

        private void Pictures(string id)
        {

        }

        private void Texts(string id)
        {

        }

        private void Defaults(string id)
        {

        }
    }
}
