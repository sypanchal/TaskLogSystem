//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc.Filters;
//using System.Web.Mvc;
//using System.Web.Routing;

//namespace TaskLogSystem.CustomFilters
//{
//    public class ResetPasswordAuthenticationFilter
//    {
//    }
//}

using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;

namespace TaskLogSystem.CustomFilters
{
    public class ResetPasswordAuthenticationFilter : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            var resetPasswordRequest = filterContext.HttpContext.Session["ResetPasswordRequest"];
            var resetPasswordRequestBy = filterContext.HttpContext.Session["ResetPasswordRequestBy"];

            if (resetPasswordRequest != null && (bool)resetPasswordRequest && resetPasswordRequestBy != null)
            {
                return;
            }
            else
            {
                // Reset password request exists, redirect to appropriate action
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        {"controller","Account"},
                        {"action","ForgotPassword"},
                    });
            }
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            // No action required here for this scenario
        }
    }
}