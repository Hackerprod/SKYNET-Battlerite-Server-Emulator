using BloodGUI_Binding.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using WebSocketSharp.Net;

namespace SKYNET.Dispatchers
{
    public class FileHandler : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {

        }

        internal void Process(string filename, HttpListenerResponse response)
        {
            string filepath = Path.Combine("Data", "Files", filename);
            if (File.Exists(filepath))
            {
                byte[] bytes = File.ReadAllBytes(filepath);
                SendResponse(response, bytes);
            }
            else
                BattleriteServer.ilog.Error($"Not found file {filename} for request");
        }
    }
}
