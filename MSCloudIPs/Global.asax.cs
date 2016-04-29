using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MSCloudIPs.Models;

namespace MSCloudIPs
{
    public class WebApiApplication : System.Web.HttpApplication
    {

        //public static System.Xml.XmlDocument Office365XML = new System.Xml.XmlDocument();
        //public static System.Xml.XmlDocument AzureXML = new System.Xml.XmlDocument();

        //really should be at the model level, but this is easier for now
        //public static DateTime LastOffice365Update;
        //public static DateTime LastAzureUpdate;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //pre-populate everything
            MicrosoftIPs.UpdateIPs(Models.CloudIP_ENum.All);
        }
    }
}
