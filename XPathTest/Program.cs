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
            string CRMOnlineurl = "https://support.microsoft.com/api/content/kb/2655102";  //CRM Online URLs
            var resutl=DoWork2(new Uri(CRMOnlineurl)).Result;
            //string serviceName = "o365";
            //string addressType = "IPv4";
            //string xpath = "/products/product[@name =\"" + serviceName + "\"]/addresslist[@type=\"" + addressType.ToString() + "\"]";
            string xpath = "";
            //XmlNodeList items = result.SelectNodes(xpath);
            //Debug.WriteLine(items.Count);
            
            //foreach (XmlNode item in items)
            //{
            //    foreach(XmlNode childNode in item.ChildNodes)
            //    {
            //        Debug.WriteLine(childNode.InnerText);
            //    }
            //}
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

        //string parsing for CRMOnline URLs
        static async System.Threading.Tasks.Task<System.Xml.XmlDocument> DoWork2(Uri url)
        {
            System.Xml.XmlDocument _result = new System.Xml.XmlDocument();
            XmlElement root = _result.CreateElement("crmonline");
            _result.AppendChild(root);
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

            //,downloadData={base_0:{url:"https://download.microsoft.com/download/0/1/8/018E208D-54F8-44CD-AA26-CD7BC9524A8C/PublicIPs_20160418.xml",id:41653,oldid:"9d8be98c-9d86-4068-8a88-51bb2e0d1ca6"}}
            String output;
            using (var stream = await client.GetStreamAsync(url))
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(stream);
                output = reader.ReadToEnd();

            }

            output = RemoveStrayHTML(output);

            //this splits everything into 7 objects
            string[] stringSeparators = new string[] { "<span class='text-base'>" };
            String[] output2 = output.Split(stringSeparators, StringSplitOptions.None);

            //since the first one is all the text *before* the URLs,
            //we can drop the first one
            foreach (String org in output2.Skip(1))
            {
                //now, split it on <br/>
                //skip the first one
                String url1;
                XmlElement region = _result.CreateElement("region");
                foreach (string url0 in org.Split(new string[] { @"<br/>" }, StringSplitOptions.None).Skip(1))
                {

                    XmlAttribute regionname = _result.CreateAttribute("name");
                    if (url0.Length > 0)
                    {
                        url1 = ReplaceNonPrintableCharacters(url0);
                        Debug.WriteLine("org: " + org.Substring(0, org.IndexOf(":")));
                        Debug.WriteLine(url1.IndexOf(@"://"));
                        if (url1.IndexOf(@"://") > 0 && url1.IndexOf(@"://") < 7)
                        {
                            url1 = url1.Substring(url1.IndexOf("h"));
                            regionname.Value = org.Substring(0, org.IndexOf("based organizations")).Replace(" area", "").Trim(); 
                            region.Attributes.Append(regionname);
                            XmlElement urlnode = _result.CreateElement("url");
                            XmlAttribute urlname = _result.CreateAttribute("name");
                            urlname.Value = url1;
                            urlnode.Attributes.Append(urlname);
                            region.AppendChild(urlnode);
                            root.AppendChild(region);
                        }

                    }
                }
            }
            return _result;
        }

        private static string RemoveStrayHTML(string output)
        {
            String _output = output;
            //remove some stray "&#160"
            _output = _output.Replace("&#160;", "");
            //remove stray <span> and </span>
            _output = _output.Replace("<span>", "").Replace(@"</span>", "");
            return output;
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
    }
}
