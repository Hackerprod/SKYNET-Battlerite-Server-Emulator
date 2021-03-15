using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace SKYNET.Network.Example
{
    public class SelfHostServer
    {
        public SelfHostServer()
        {
            string text = $"http://localhost:{9090}/";
            HttpSelfHostConfiguration httpSelfHostConfiguration = new HttpSelfHostConfiguration(text);

            httpSelfHostConfiguration.Formatters.Remove(httpSelfHostConfiguration.Formatters.XmlFormatter);
            httpSelfHostConfiguration.Routes.MapHttpRoute("API Default", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            httpSelfHostConfiguration.Routes.MapHttpRoute("steam-async", "auth/steam-async/v1", new { id = RouteParameter.Optional });
            
            HttpSelfHostServer server = new HttpSelfHostServer(httpSelfHostConfiguration);
            server.OpenAsync().Wait();

            //api/products	Get a list of all products.
            //api/products/id	Get a product by ID.
            //api/products/?category=category	Get a list of products by category.
        }
    }
}
