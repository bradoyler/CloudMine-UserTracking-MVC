using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Net;
using System.Web.Script.Serialization;
using System.Text;
using System.IO;

namespace CloudMine_UserTracker
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class UserTrackingAttribute : ActionFilterAttribute
    {
        public static string appId = "1232131";
        public static string appSecret = "1232131";

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            // if (!context.HttpContext.User.Identity.IsAuthenticated) return;
            // var uId = new CurrentUser(context.HttpContext).person.Id;
            var uname = context.HttpContext.User.Identity.Name;
            var request = context.HttpContext.Request;

            var refUrl = "";
            if (request.UrlReferrer != null) refUrl = request.UrlReferrer.ToString();

            PageRequest pv = new PageRequest
            {
                Name = request.RawUrl,
                Url = request.RawUrl,
                UrlReferrer = refUrl,
                UserIP = request.UserHostAddress,
                UserName = uname,
                // UserId = uId, CookieVal = reqInfo.Cookies["yourcookie"].Value,
                Created = ExtensionMethods.JsonDate(DateTime.Now),
                Action = (string)context.RequestContext.RouteData.Values["action"],
                Controller = (string)context.RouteData.Values["controller"],
                Param1 = (string)context.RouteData.Values["uref"],
                PageId = Convert.ToInt32(context.RouteData.Values["Id"]),
            };

            LogPageView(pv);
        }

        private static void LogPageView(PageRequest preq)
        {
            try
            {
                WebClient client = new WebClient();
                WebRequest request = WebRequest.Create("https://api.cloudmine.me/v1/app/"+ appId +"/text");
                request.Method = "PUT";
                request.Headers.Add("X-CloudMine-ApiKey", appSecret);
                request.ContentType = "application/json";
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var data = serializer.Serialize(preq);

                var payload = "{\"PV_" + DateTime.Now.Ticks + "\":" + data + "}";
                byte[] byteArray = Encoding.UTF8.GetBytes(payload);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                var response = request.GetResponse();
            }
            catch (Exception e)
            {
                //log error 
            }
        }

        class PageRequest
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int PageId { get; set; }
            public Double Created { get; set; }
            public string CookieVal { get; set; }
            public string Url { get; set; }
            public string UserName { get; set; }
            public int UserId { get; set; }
            public string UserIP { get; set; }
            public string UrlReferrer { get; set; }
            public string UserHostName { get; set; }
            public string UserAgent { get; set; }
            public string Param1 { get; set; }
            public string Controller { get; set; }
            public string Action { get; set; }
        }
    }

    public static class ExtensionMethods
    {
        public static double JsonDate(this DateTime dt)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return Math.Round(ts.TotalMilliseconds, 0);
        }
    }
}