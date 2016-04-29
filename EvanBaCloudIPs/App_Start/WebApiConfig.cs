﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace EvanBaCloudIPs
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}/{type}",
                defaults: new { id = RouteParameter.Optional, type=RouteParameter.Optional}
                );

            config.Routes.MapHttpRoute(
                name: "MetaDataApi",
                routeTemplate: "api/{controller}/{action}"
            );
        }
    }
}
