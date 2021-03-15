using BloodGUI_Binding.Web;
using StunGUI;
using StunShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SKYNET.Dispatchers
{
    public class Storage : MsgHandler
    {
        public override void AddHandlers(MsgDispatcher dispatcher)
        {
            dispatcher["docs/get/v1"] = OnDocsGetV1;

        }
        private void OnDocsGetV1(RequestMessage request)
        {
            //ShopDocument
            GetDocsRequest Request = request.Body.Deserialize<GetDocsRequest>();
            string Data = "";
            switch (Request.documentType)
            {
                case DocumentType.UNKNOWN:
                    break;
                case DocumentType.SHOP:
                    if (File.Exists(Path.Combine("Data", "Files", "JSON", "ShopDocument.def.json")))
                    {
                        Data = File.ReadAllText(Path.Combine("Data", "Files", "JSON", "ShopDocument.def.json"));
                    }
                    //string hash = HashHelper.GetMD5Hash(this.DocumentString), Request.expectedhash;
                    ShopDocument document = new ShopDocument();
                    break;
                case DocumentType.QUESTS:
                    break;
                default:
                    break;
            }
            GetDocsResponse data = new GetDocsResponse()
            {
                documentType = Request.documentType,
                data = Data
            };
            SendResponse(request.ListenerResponse, data);
        }
    }
}
