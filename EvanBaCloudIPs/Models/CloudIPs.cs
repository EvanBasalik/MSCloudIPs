using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Threading.Tasks;

namespace EvanBaCloudIPs.Models
{

    public enum CloudIP_ENum
    {
        Azure=1,
        Office365=2
    }

    public class Office365IPs : MicrosoftIPs
    {
        //http://go.microsoft.com/fwlink/?LinkId=533185
        public Office365IPs()
        {
            this.CloudIPType = CloudIP_ENum.Office365;
        }
    }

    public class AzureIPs : MicrosoftIPs
    {
        //https://www.microsoft.com/en-us/download/confirmation.aspx?id=41653
        public AzureIPs()
        {
            this.CloudIPType = CloudIP_ENum.Azure;
        }
    }


    public class MicrosoftIPs : System.Collections.ObjectModel.Collection<String>
    {
        public CloudIP_ENum CloudIPType { get; set; }

        public XmlDocument AzureIPs = new XmlDocument();
        public XmlDocument Office365IPs = new XmlDocument();

        public async System.Threading.Tasks.Task<System.Xml.XmlDocument> GetIPs(Uri url)
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

        public void UpdateIPs()
        {
            //Azure is currently a multi-step process because we embed the actual link in the web page rather than a 
            //direct pointer
            Task<System.Xml.XmlDocument> AzureStep1 = this.GetIPs(new Uri("https://www.microsoft.com/en-us/download/confirmation.aspx?id=41653"));
            System.Threading.Tasks.Task<System.Xml.XmlDocument> Office365Step1 = this.GetIPs(new Uri("http://go.microsoft.com/fwlink/?LinkId=533185"));
            System.Threading.Tasks.Task.WaitAll(AzureStep1, Office365Step1);
            String AzureStep2 = AzureStep1.Result.InnerText.Substring(AzureStep1.Result.InnerText.IndexOf("downloadData={base_0:{url:") + 27, AzureStep1.Result.InnerText.IndexOf("PublicIPs_") + 20 - 25 - AzureStep1.Result.InnerText.IndexOf("downloadData={base_0:{url:"));

            System.Threading.Tasks.Task<System.Xml.XmlDocument> AzureStep3 = this.GetIPs(new Uri(AzureStep2));
            System.Threading.Tasks.Task.WaitAll(AzureStep3);

            Office365IPs = Office365Step1.Result;
            AzureIPs = AzureStep3.Result;
        }
}
    }
