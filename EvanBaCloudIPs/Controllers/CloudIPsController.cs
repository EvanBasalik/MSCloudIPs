using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EvanBaCloudIPs.Models;
using System.Xml;

namespace EvanBaCloudIPs.Controllers
{
    public class AzureIPsController : ApiController
    {
        AzureIPs IPs = new AzureIPs();

        // api/azureips/
        public IEnumerable<String> GetIPs()
        {
            return IPs;
        }

        // /api/azureips?regionname=europewest
        public IEnumerable<String> GetIPsbyService(string regionName)
        { 
            AzureIPs _IPs = new AzureIPs();
            System.Collections.ObjectModel.Collection<string> IPs = new System.Collections.ObjectModel.Collection<string>();
            string xpath = "/AzurePublicIpAddresses/Region[@Name =\"" + regionName + "\"]";
            XmlNodeList addresses = WebApiApplication.AzureXML.SelectNodes(xpath);

            foreach (XmlNode address in addresses)
            {
                foreach (XmlNode childNode in address.ChildNodes)
                {
                    _IPs.Add(childNode.Attributes.GetNamedItem("Subnet").Value);
                }
            }
            return _IPs;
        }

        // /api/azureips?action=listregions
        [HttpGet]
        public IHttpActionResult Actions(string action)
        {
            if (action == "listregions")
            {
                System.Collections.ObjectModel.Collection<string> regions = new System.Collections.ObjectModel.Collection<string>();
                foreach (XmlNode product in WebApiApplication.AzureXML.LastChild.ChildNodes)
                {
                    regions.Add(product.Attributes.GetNamedItem("Name").Value);
                }
                return Ok(regions);
            }
            //else if (action == "update") 
            //{
            //    IPs.UpdateIPs();
            //    return Ok();
            //}
            else
            {
                return NotFound();
            }


        }
    }
    public class Office365IPsController : ApiController
    {
        Office365IPs IPs = new Office365IPs();

        // api/office365ips/
        public IEnumerable<String> GetIPs()
        {
            Office365IPs _IPs = new Office365IPs();
            return _IPs;
        }

        public enum addressTypeEnum
        {
            IPv4,
            IPv6,
            URL
        }

        // /api/office365ips?servicename=o365&addresstype=ipv4
        public IEnumerable<String> GetIPsbyService(string serviceName, string addressType)
        {
            Office365IPs _IPs = new Office365IPs();
            addressTypeEnum _addressType;
            if (Enum.TryParse(addressType, true, out _addressType))
            {
                System.Collections.ObjectModel.Collection<string> IPs = new System.Collections.ObjectModel.Collection<string>();
                string xpath = "/products/product[@name =\"" + serviceName + "\"]/addresslist[@type=\"" + _addressType.ToString() + "\"]";
                XmlNodeList addresses = WebApiApplication.Office365XML.SelectNodes(xpath);

                foreach (XmlNode address in addresses)
                {
                    foreach (XmlNode childNode in address.ChildNodes)
                    {
                        _IPs.Add(childNode.InnerText);
                    }
                }
            }
            return _IPs;
        }

        // /api/office365ips?action=listservices
        [HttpGet]
        public IHttpActionResult Actions(string action)
        {
            if (action == "listservices")
            {
                System.Collections.ObjectModel.Collection<string> services = new System.Collections.ObjectModel.Collection<string>();
                foreach(XmlNode product in WebApiApplication.Office365XML.LastChild.ChildNodes)
                {
                    services.Add(product.Attributes.GetNamedItem("name").Value);
                }
                return Ok(services);
            }
            //else if (action == "update")
            //{
            //    IPs.UpdateIPs();
            //    return Ok();
            //}
            else
            {
                return NotFound();
            }
            

        }

        //public IHttpActionResult GetProduct(int id)
        //{
        //    var product = products.FirstOrDefault((p) => p.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(product);
        //}
    }
}
