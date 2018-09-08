using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.Http
{
    public class ResponseHelper
    {
        private HttpListenerResponse response;
        public ResponseHelper(HttpListenerResponse response)
        {
            this.response = response;
            OutputStream = response.OutputStream;
        }

        public Stream OutputStream { get; set; }
        public class FileObject
        {
            public FileStream fs;
            public byte[] buffer;
        }

        public void WriteToClient(FileStream fs)
        {
            response.StatusCode = 200;
            byte[] buffer = new byte[1024];
            FileObject obj = new FileObject() { fs = fs, buffer = buffer };
            fs.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(EndWrite), obj);
        }

        void EndWrite(IAsyncResult ar)
        {
            var obj = ar.AsyncState as FileObject;
            var num = obj.fs.EndRead(ar);
            OutputStream.Write(obj.buffer, 0, num);
            if (num < 1)
            {
                obj.fs.Close(); //关闭文件流
                OutputStream.Close(); //关闭输出流，如果不关闭，浏览器将一直在等待状态
                return;
            }
            obj.fs.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(EndWrite), obj);
        }

        public void ResponseToClient(string body)
        {
            response.ContentType = "application/json; charset=UTF-8";
            OutputStream.Write(Encoding.UTF8.GetBytes(body), 0, Encoding.UTF8.GetBytes(body).Length);
            OutputStream.Close();
        }

        public void ResponseToClient()
        {
            OutputStream.Close();
        }
    }
}
