using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Threading.Tasks;

namespace EvanBaCloudIPs
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static System.Xml.XmlDocument Office365XML = new System.Xml.XmlDocument();
        public static System.Xml.XmlDocument AzureXML = new System.Xml.XmlDocument();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            EvanBaCloudIPs.Models.MicrosoftIPs IPs = new Models.MicrosoftIPs();
            IPs.UpdateIPs();
            Office365XML = IPs.Office365IPs;
            AzureXML = IPs.AzureIPs;
        }
    }

}
