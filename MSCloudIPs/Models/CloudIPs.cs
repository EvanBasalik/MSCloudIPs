using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Xml;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

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
        public static XmlDocument CRMOnlineIPs = new XmlDocument();

        public static DateTime LastAzureIPUpdate;
        public static DateTime LastO365IPUpdate;
        public static DateTime LastCRMOnlineIPUpdate;


        public static async System.Threading.Tasks.Task<System.Xml.XmlDocument> GetIPsasXmlDocument(Uri url)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

            XmlDocument doc = new XmlDocument();
            using (var stream = await client.GetStreamAsync(url))
            {
                doc.Load(stream);
            }
            return doc;
        }

        public static async System.Threading.Tasks.Task<String> GetIPsasString(Uri url)
        {
            String _result;
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            using (var stream = await client.GetStreamAsync(url))
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(stream);
                _result = reader.ReadToEnd();

            }
            return _result;
        }

        public static void UpdateIPs(CloudIP_ENum cloudIPstoUpdate, bool force=false)
        {
            switch (cloudIPstoUpdate)
            {
                case CloudIP_ENum.All:
                    System.Threading.Tasks.Task.WaitAll(UpdateAzureIPs(force), UpdateO365IPs(force), UpdateCRMOnlineIPs(force));
                    break;
                case CloudIP_ENum.Azure:
                    System.Threading.Tasks.Task.WaitAll(UpdateAzureIPs(force));
                    break;
                case CloudIP_ENum.Office365:
                    System.Threading.Tasks.Task.WaitAll(UpdateO365IPs(force));
                    break;
                case CloudIP_ENum.CRMOnline:
                    System.Threading.Tasks.Task.WaitAll(UpdateCRMOnlineIPs(force));
                    break;
            }

        }

        private static async System.Threading.Tasks.Task UpdateCRMOnlineIPs(bool force)
        {
            if (LastCRMOnlineIPUpdate.AddHours(12) <= DateTime.Now | force)
            {
                String CRMOnlineStep1 = await GetIPsasString(new Uri("https://support.microsoft.com/api/content/kb/2655102"));
                System.Xml.XmlDocument _result = new System.Xml.XmlDocument();
                XmlElement root = _result.CreateElement("crmonline");
                _result.AppendChild(root);
                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

                String CRMOnlineStep2 = RemoveStrayHTML(CRMOnlineStep1);

                //this splits everything into 7 objects
                string[] stringSeparators = new string[] { "<span class='text-base'>" };
                String[] CRMOnlineStep3 = CRMOnlineStep2.Split(stringSeparators, StringSplitOptions.None);

                //since the first one is all the text *before* the URLs,
                //we can drop the first one
                foreach (String org in CRMOnlineStep3.Skip(1))
                {
                    //now, split it on <br/>
                    //skip the first one
                    String url1;
                    XmlElement region = _result.CreateElement("region");
                    XmlAttribute regionname = _result.CreateAttribute("name");
                    regionname.Value = org.Substring(0, org.IndexOf("based organizations")).Replace(" area", "").Trim();
                    region.Attributes.Append(regionname);

                    //build the addresslist type attribute
                    XmlElement addressTypeNode = _result.CreateElement("addresslist");
                    XmlAttribute addressTypeName = _result.CreateAttribute("type");
                    addressTypeName.Value = "url";
                    addressTypeNode.Attributes.Append(addressTypeName);

                    foreach (string url0 in org.Split(new string[] { @"<br/>" }, StringSplitOptions.None).Skip(1))
                    {
                        if (url0.Length > 0)
                        {
                            url1 = ReplaceNonPrintableCharacters(url0);
                            System.Diagnostics.Debug.WriteLine("org: " + org.Substring(0, org.IndexOf(":")));
                            System.Diagnostics.Debug.WriteLine(url1.IndexOf(@"://"));
                            if (url1.IndexOf(@"://") > 0 && url1.IndexOf(@"://") < 7)
                            {
                                url1 = url1.Substring(url1.IndexOf("h"));
                                XmlElement urlnode = _result.CreateElement("address");
                                urlnode.InnerText = url1;
                                addressTypeNode.AppendChild(urlnode);

                            }

                        }
                    }
                    region.AppendChild(addressTypeNode);
                    root.AppendChild(region);
                }

                CRMOnlineIPs = _result;
                LastCRMOnlineIPUpdate = DateTime.UtcNow;
            }
        }

        private static string RemoveStrayHTML(string output)
        {
            String _output = output;
            //remove some stray "&#160"
            _output = _output.Replace("&#160;", "");
            //remove stray <span> and </span>
            _output = _output.Replace("<span>", "").Replace(@"</span>", "");
            return _output;
        }

        static string ReplaceNonPrintableCharacters(string s)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                byte b = (byte)c;
                if (b > 32 & !Char.IsWhiteSpace(c))
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        //limit each variant to twice per day
        //set force=true to make it run regardless
        //a common scenario for force=true would be on application startup
        private static async System.Threading.Tasks.Task UpdateO365IPs(bool force)
        {
            if (LastO365IPUpdate.AddHours(12) <= DateTime.Now |force)
            {
                System.Xml.XmlDocument Office365Step1 = await GetIPsasXmlDocument(new Uri("http://go.microsoft.com/fwlink/?LinkId=533185"));
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
                System.Xml.XmlDocument AzureStep1 = await GetIPsasXmlDocument(new Uri("https://www.microsoft.com/en-us/download/confirmation.aspx?id=41653"));
                String AzureStep2 = AzureStep1.InnerText.Substring(AzureStep1.InnerText.IndexOf("downloadData={base_0:{url:") + 27, AzureStep1.InnerText.IndexOf("PublicIPs_") + 20 - 25 - AzureStep1.InnerText.IndexOf("downloadData={base_0:{url:"));
                System.Xml.XmlDocument AzureStep3 = await GetIPsasXmlDocument(new Uri(AzureStep2));
                AzureIPs = AzureStep3;
                LastAzureIPUpdate = DateTime.UtcNow;
            }

        }
    }
}