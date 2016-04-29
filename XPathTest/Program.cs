using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Web.Http;
using System.Diagnostics;

namespace XPathTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string O365url = "http://go.microsoft.com/fwlink/?LinkId=533185";
            var result=DoWork(new Uri(O365url)).Result;
            //string serviceName = "o365";
            //string addressType = "IPv4";
            //string xpath = "/products/product[@name =\"" + serviceName + "\"]/addresslist[@type=\"" + addressType.ToString() + "\"]";
            string xpath = "";
            XmlNodeList items = result.SelectNodes(xpath);
            Debug.WriteLine(items.Count);
            
            foreach (XmlNode item in items)
            {
                foreach(XmlNode childNode in item.ChildNodes)
                {
                    Debug.WriteLine(childNode.InnerText);
                }
            }
        }

        static async System.Threading.Tasks.Task<System.Xml.XmlDocument> DoWork(Uri url)
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
    }
}
