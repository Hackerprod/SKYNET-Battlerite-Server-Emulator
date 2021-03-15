using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using WebSocketSharp.Net;

public static class Extentions
{

    public static T Deserialize<T>(this byte[] Body)
    {
        try
        {
            string json = Encoding.Default.GetString(Body);
            return new JavaScriptSerializer().Deserialize<T>(json);
        }
        catch 
        {
            return (T)default;
        }
    }
    public static byte[] GetBody(this HttpListenerRequest request)
    {
        try
        {
            Stream inputStream = request.InputStream;
            MemoryStream bodyStream = new MemoryStream();
            using (Stream stream = inputStream)
            {
                stream.CopyTo(bodyStream);
            }
            return bodyStream.ToArray();
        }
        catch
        {
            return (byte[])default;
        }
    }
    public static byte[] GetBody(this System.Net.HttpListenerRequest request)
    {
        try
        {
            Stream inputStream = request.InputStream;
            MemoryStream bodyStream = new MemoryStream();
            using (Stream stream = inputStream)
            {
                stream.CopyTo(bodyStream);
            }
            return bodyStream.ToArray();
        }
        catch
        {
            return (byte[])default;
        }
    }


}
