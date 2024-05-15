//using NLog;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace TaskLogSystem.CustomFilters
//{
//    public class ActionLogFilter
//    {
//    }
//}


using System;
using System.Web.Mvc;
using NLog;
using TaskLogSystem.Models;

public class ActionLogFilter : ActionFilterAttribute
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
        var action = filterContext.ActionDescriptor.ActionName;
        var user = (Employee)filterContext.HttpContext.Session["CurrentUser"];
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        if (controller == "Account")
        {
            if (action == "Login" && filterContext.HttpContext.Session["CurrentUser"] is Employee LoggedInUser)
            {
                // Log user login
                Logger.Info($"{LoggedInUser.FirstName} {LoggedInUser.LastName} with email {LoggedInUser.Email} has logged in at {timestamp}");
            }
            return;
        }

        try
        {
            // Your action execution code here
            base.OnActionExecuted(filterContext);

            // Log info message
            var logMessage = $"{user.FirstName} {user.LastName} performed action '{action}' in controller '{controller}' at {timestamp}";
            Logger.Info(logMessage);
        }
        catch (Exception ex)
        {
            // Log error message
            var errorMessage = $"{user.FirstName} {user.LastName} encountered an error while performing action '{action}' in controller '{controller}' at {timestamp}. Error: {ex.Message}";
            Logger.Error(ex, errorMessage);
        }
    }
}