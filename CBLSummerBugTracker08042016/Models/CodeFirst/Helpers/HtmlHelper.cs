using System;
using System.Web.Mvc;
using System.Web.WebPages.Html;

namespace CBLSummerBugTracker08042016.Models.CodeFirst.Helpers
{ 
    public static class HtmlHelper
    {

        public static string IsSelected(this System.Web.Mvc.HtmlHelper html, string controller = null, string action = null, string cssClass = null)
        {
            if (String.IsNullOrEmpty(cssClass))
                cssClass = "active";

            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

            if (String.IsNullOrEmpty(controller))
                controller = currentController;

            if (String.IsNullOrEmpty(action))
                action = currentAction;

            return controller == currentController && action == currentAction ?
                cssClass : String.Empty;
        }

        public static string PageClass(this System.Web.Mvc.HtmlHelper html)
        {
            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            return currentAction;
        }


    }
}
