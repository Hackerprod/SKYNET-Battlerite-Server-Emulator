using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace SKYNET
{
    public class WebApi
    {
        private ILog iLog => BattleriteServer.ilog;
        private HttpServer httpsv;
        private int ApiPort;

        MsgDispatcher Dispatcher = BattleriteServer.MsgDispatcher;

        public WebApi(int port)
        {
            httpsv = new HttpServer(port);
            httpsv.OnGet += ProcessRequest;
            httpsv.OnPost += ProcessRequest;
            httpsv.ReuseAddress = true;
            ApiPort = port;
        }

        private void ProcessRequest(object sender, HttpRequestEventArgs e)
        {
            try
            {
                string requestHandlerName = e.Request.RawUrl.Substring(1);
                
                
                /*foreach (var query in e.Request.QueryString)
                {
                    BattleriteServer.ilog.Error(query);
                }*/
                //byte[] bytes = GetBody(e.Request.InputStream);
                //string xml = Encoding.UTF8.GetString(bytes);
                //BattleriteServer.ilog.Error(xml);

                if (IsFileRequest(requestHandlerName, out string ext))
                {
                    ProcessFileRequest(e, ext);
                }
                else
                    ProcessRequest(e);



            }
            catch (Exception ex)
            {
                BattleriteServer.ilog.Error(ex.Message);
            }
        }
        private void ProcessRequest(HttpRequestEventArgs e)
        {
            //e.Request.
            var request = e.Request;
            var response = e.Response;
            string queryString = e.Request.Url.Query;

            string requestHandlerName = e.Request.RawUrl.Substring(1);
            if (requestHandlerName.Contains("?"))
            {
                requestHandlerName = requestHandlerName.Split('?')[0];
            }
            Test(request.InputStream);

            byte[] bytes = request.GetBody();

            

            if (!requestHandlerName.Contains("Chat"))
            {
                //BattleriteServer.ilog.Debug(Encoding.UTF8.GetString(bytes));
            }
            

            RequestMessage message = new RequestMessage()
            {
                Query = queryString,
                RequestHandler = requestHandlerName,
                ListenerResponse = response,
                Body = bytes,
                RemoteAddress = request.RemoteEndPoint.Address,
                RequestType = (e.Request.HttpMethod == "GET") ? RequestType.GET : RequestType.POST
            };
            Dispatcher.Dispatch(message);

        }

        private void Test(Stream s)
        {
            //StreamReader reader = new StreamReader(s);
            //string data = reader.ReadToEnd();
            //BattleriteServer.ilog.Error(data);
        }

        private void ProcessFileRequest(HttpRequestEventArgs e, string ext)
        {
            var request = e.Request;
            var response = e.Response;
            string filefolder = "";

            switch (ext)
            {
                case ".json":
                    filefolder = "JSON";
                    break;
                case ".sjson":
                    filefolder = "JSON";
                    break;
                case ".png":
                    filefolder = "Images";
                    break;
                case ".jpg":
                    filefolder = "Images";
                    break;
                default:
                    filefolder = "";
                    break;
            }

            string requestfilename = e.Request.RawUrl.Substring(1);
            string filename = Path.GetFileName(requestfilename);
            //BattleriteServer.ilog.Info(requestfilename);
            BattleriteServer.ilog.Info("Received request for file " + filename);

            if (string.IsNullOrEmpty(filefolder))
                BattleriteServer.Instance.fileHandler.Process(filename, response);
            else
                BattleriteServer.Instance.fileHandler.Process(filefolder + "/" + filename, response);
        }

        private bool IsFileRequest(string requestHandler, out string ext)
        {
            if (requestHandler.Length > 5)
            {
                string extension = Path.GetExtension(requestHandler);
                switch (extension)
                {
                    case ".json":
                        ext = ".json";
                        return true;
                    case ".sjson":
                        ext = ".sjson";
                        return true;
                    case ".png":
                        ext = ".png";
                        return true;
                    case ".jpg":
                        ext = ".jpg";
                        return true;
                    default:
                        ext = "";
                        return false;
                }
            }
            ext = "";
            return false;
        }



        private byte[] GetBody(Stream inputStream)
        {
            MemoryStream bodyStream = new MemoryStream();
            using (Stream stream = inputStream)
            {
                stream.CopyTo(bodyStream);
            }
            return bodyStream.ToArray();
        }
        public bool Start()
        {
            try
            {
                httpsv.Start();
                BattleriteServer.ilog.Info((object)"WebApi server is online on port 25000");
                return true;
            }
            catch
            {
                BattleriteServer.ilog.Error("Error starting WebApi server on port 25000");
                return false;
            }
        }

        internal void Stop()
        {
            httpsv.Stop();
        }
    }
    public enum RequestType
    {
        GET,
        POST
    }
}
