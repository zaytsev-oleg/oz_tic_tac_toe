using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace krestiki_noliki
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{row}/{col}",
                defaults: new { controller = "Home", action = "Index", row = UrlParameter.Optional, col = UrlParameter.Optional }
            );
        }
    }
}