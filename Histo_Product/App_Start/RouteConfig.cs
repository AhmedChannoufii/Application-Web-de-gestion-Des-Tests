using System.Web.Mvc;
using System.Web.Routing;

namespace Histo_Product
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Log", action = "Login", id = UrlParameter.Optional }
        );

        }
    }
}