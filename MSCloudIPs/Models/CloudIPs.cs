using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Xml;
using System.Threading.Tasks;

namespace MSCloudIPs.Models
{
    public enum CloudIP_ENum
    {
        Azure = 1,
        CRMOnline=2,
        Office365 = 4,
        All=8
    }

    public static class MicrosoftIPs
    {
        public static CloudIP_ENum CloudIPType { get; set; }

        public static Dictionary<string, string> O365Services
        {
            get
            {
                Dictionary<string, string> _o365Services = new Dictionary<string, string>();
                string xpath = "/products/product";
                XmlNodeList addresses = MicrosoftIPs.Office365IPs.SelectNodes(xpath);

                foreach (XmlNode address in addresses)
                {
                    _o365Services.Add(address.Attributes.GetNamedItem("name").Value.ToLower(), address.Attributes.GetNamedItem("name").Value);
                }
                return _o365Services;
            }

        }

        public static Dictionary<string, string> AzureRegions
        {
            get
            {
                Dictionary<string, string> _azureRegions = new Dictionary<string, string>();
                string xpath = "/AzurePublicIpAddresses/Region";
                XmlNodeList addresses = MicrosoftIPs.AzureIPs.SelectNodes(xpath);

                foreach (XmlNode address in addresses)
                {
                    _azureRegions.Add(address.Attributes.GetNamedItem("Name").Value.ToLower(), address.Attributes.GetNamedItem("Name").Value);
                }

                return _azureRegions;
            }
        }

        public static XmlDocument AzureIPs = new XmlDocument();
        public static XmlDocument Office365IPs = new XmlDocument();

        public static DateTime LastAzureIPUpdate;
        public static DateTime LastO365IPUpdate;


        public static async System.Threading.Tasks.Task<System.Xml.XmlDocument> GetIPs(Uri url)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

            //,downloadData={base_0:{url:"https://download.microsoft.com/download/0/1/8/018E208D-54F8-44CD-AA26-CD7BC9524A8C/PublicIPs_20160418.xml",id:41653,oldid:"9d8be98c-9d86-4068-8a88-51bb2e0d1ca6"}}
            XmlDocument doc = new XmlDocument();
            using (var stream = await client.GetStreamAsync(url))
            {
                doc.Load(stream);
            }
            return doc;
        }

        public static void UpdateIPs(CloudIP_ENum cloudIPstoUpdate, bool force=false)
        {
            switch (cloudIPstoUpdate)
            {
                case CloudIP_ENum.All:
                    System.Threading.Tasks.Task.WaitAll(UpdateAzureIPs(force), UpdateO365IPs(force));
                    break;
                case CloudIP_ENum.Azure:
                    System.Threading.Tasks.Task.WaitAll(UpdateAzureIPs(force));
                    break;
                case CloudIP_ENum.Office365:
                    System.Threading.Tasks.Task.WaitAll(UpdateO365IPs(force));
                    break;
            }

        }

        //limit each variant to twice per day
        //set force=true to make it run regardless
        //a common scenario for force=true would be on application startup
        private static async System.Threading.Tasks.Task UpdateO365IPs(bool force)
        {
            if (LastO365IPUpdate.AddHours(12) <= DateTime.Now |force)
            {
                System.Xml.XmlDocument Office365Step1 = await GetIPs(new Uri("http://go.microsoft.com/fwlink/?LinkId=533185"));
                Office365IPs = Office365Step1;
                LastO365IPUpdate = DateTime.UtcNow;
            }
        }

        private static async System.Threading.Tasks.Task UpdateAzureIPs(bool force)
        {
            if (LastAzureIPUpdate.AddHours(12) <=DateTime.Now | force)
            {
                //Azure is currently a multi-step process because we embed the actual link in the web page rather than a 
                //direct pointer
                System.Xml.XmlDocument AzureStep1 = await GetIPs(new Uri("https://www.microsoft.com/en-us/download/confirmation.aspx?id=41653"));
                String AzureStep2 = AzureStep1.InnerText.Substring(AzureStep1.InnerText.IndexOf("downloadData={base_0:{url:") + 27, AzureStep1.InnerText.IndexOf("PublicIPs_") + 20 - 25 - AzureStep1.InnerText.IndexOf("downloadData={base_0:{url:"));
                System.Xml.XmlDocument AzureStep3 = await GetIPs(new Uri(AzureStep2));
                AzureIPs = AzureStep3;
                LastAzureIPUpdate = DateTime.UtcNow;
            }

        }
    }
}