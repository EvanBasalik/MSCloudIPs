using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MSCloudIPs.Models;
using System.Xml;

namespace MSCloudIPs.Controllers
{
    public class CloudIPController:ApiController
    {

    }

    [RoutePrefix("api/azureips")]
    public class AzureIPsController : CloudIPController
    {

        /// <summary>
        /// List all Azure IPs
        /// </summary>
        /// <example>api/azureips/all</example>
        [Route("all")]
        [HttpGet]
        public IEnumerable<String> GetIPs()
        {
            System.Collections.ObjectModel.Collection<string> _IPs = new System.Collections.ObjectModel.Collection<string>();
            string xpath = "/AzurePublicIpAddresses/Region";
            XmlNodeList addresses = MicrosoftIPs.AzureIPs.SelectNodes(xpath);

            foreach (XmlNode address in addresses)
            {
                foreach (XmlNode childNode in address.ChildNodes)
                {
                    _IPs.Add(childNode.Attributes.GetNamedItem("Subnet").Value);
                }
            }
            return _IPs;
        }

        /// <summary>
        /// Azure IPs for the specified region
        /// </summary>
        /// <param name="regionName">Target region</param>
        /// <example>api/azureips/region/europewest</example>
        [Route("region/{regionname}")]
        [HttpGet]
        public IEnumerable<String> GetIPsbyService(string regionName)
        {
            //turn whatever was passed into the correct case as expected by the XML
            regionName = MicrosoftIPs.AzureRegions[regionName.ToLower()];

            System.Collections.ObjectModel.Collection<string> _IPs = new System.Collections.ObjectModel.Collection<string>();
            string xpath = "/AzurePublicIpAddresses/Region[@Name =\"" + regionName + "\"]";
            XmlNodeList addresses = MicrosoftIPs.AzureIPs.SelectNodes(xpath);

            foreach (XmlNode address in addresses)
            {
                foreach (XmlNode childNode in address.ChildNodes)
                {
                    _IPs.Add(childNode.Attributes.GetNamedItem("Subnet").Value);
                }
            }
            return _IPs;
        }

        /// <summary>
        /// Lists all the Azure regions which have published data.
        /// </summary>
        /// <param name="operation">Valid values: listregions</param>
        /// <example>api/azureips/operation/listregions</example>
        [Route("operation/{operation}")]
        [HttpGet]
        public IHttpActionResult Actions(string operation)
        {
            System.Collections.ObjectModel.Collection<string> _IPs = new System.Collections.ObjectModel.Collection<string>();
            if (operation == "listregions")
            {
                System.Collections.ObjectModel.Collection<string> regions = new System.Collections.ObjectModel.Collection<string>();
                foreach (XmlNode product in MicrosoftIPs.AzureIPs.LastChild.ChildNodes)
                {
                    regions.Add(product.Attributes.GetNamedItem("Name").Value);
                }
                return Ok(regions);
            }
            else if (operation == "update")
            {
                //MicrosoftIPs.LastAzureIPUpdate = WebApiApplication.LastAzureUpdate;
                MicrosoftIPs.UpdateIPs(CloudIP_ENum.Azure);
                return Ok(MicrosoftIPs.LastAzureIPUpdate);
            }
            else
            {
                return NotFound();
            }


        }
    }

    [RoutePrefix("api/office365ips")]
    public class Office365IPsController : ApiController
    {

        /// <summary>
        /// List all O365 IPs
        /// </summary>
        /// <example>api/office365ips/all</example>
        [Route("all")]
        [HttpGet]
        public IEnumerable<String> GetIPs()
        {
            System.Collections.ObjectModel.Collection<string> _IPs = new System.Collections.ObjectModel.Collection<string>();
            string xpath = "/products/product/addresslist";
            XmlNodeList addresses = MicrosoftIPs.Office365IPs.SelectNodes(xpath);

            foreach (XmlNode address in addresses)
            {
                foreach (XmlNode childNode in address.ChildNodes)
                {
                    _IPs.Add(childNode.InnerText);
                }
            }
            return _IPs;
        }

        public enum addressTypeEnum
        {
            IPv4,
            IPv6,
            URL
        }

        /// <summary>
        /// Office 365 IPs or URLs for the specified services
        /// </summary>
        /// <param name="serviceName">Target service</param>
        /// <param name="addressType">IPv4, IPv6, or URL</param>
        /// <example>api/office365ips/servicename/o365/addresstype/ipv4</example>
        [Route("service/{servicename}/addresstype/{addresstype}")]
        [HttpGet]
        public IEnumerable<String> GetIPsbyService(string serviceName, string addressType)
        {
            //turn whatever was passed into the correct case as expected by the XML
            serviceName = MicrosoftIPs.O365Services[serviceName.ToLower()];

            System.Collections.ObjectModel.Collection<string> _IPs = new System.Collections.ObjectModel.Collection<string>();
            addressTypeEnum _addressType;
            if (Enum.TryParse(addressType, true, out _addressType))
            {
                System.Collections.ObjectModel.Collection<string> IPs = new System.Collections.ObjectModel.Collection<string>();
                string xpath = "/products/product[@name =\"" + serviceName + "\"]/addresslist[@type=\"" + _addressType.ToString() + "\"]";
                XmlNodeList addresses = MicrosoftIPs.Office365IPs.SelectNodes(xpath);

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

        /// <summary>
        /// Lists all the Office 365 services which have published data.
        /// </summary>
        /// <param name="operation">Valid values: listservices</param>
        /// <example>api/office365ips/operation/listservices</example>
        [Route("operation/{operation}")]
        [HttpGet]
        public IHttpActionResult Actions(string operation)
        {
            System.Collections.ObjectModel.Collection<string> _IPs = new System.Collections.ObjectModel.Collection<string>();

            if (operation == "listservices")
            {
                System.Collections.ObjectModel.Collection<string> services = new System.Collections.ObjectModel.Collection<string>();
                foreach (XmlNode product in MicrosoftIPs.Office365IPs.LastChild.ChildNodes)
                {
                    services.Add(product.Attributes.GetNamedItem("name").Value);
                }
                return Ok(services);
            }
            else if (operation == "update")
            {

                //MicrosoftIPs.LastO365IPUpdate = WebApiApplication.LastOffice365Update;
                MicrosoftIPs.UpdateIPs(CloudIP_ENum.Office365);
                return Ok(MicrosoftIPs.LastO365IPUpdate);
            }
            else
            {
                return NotFound();
            }


        }

    }
}
