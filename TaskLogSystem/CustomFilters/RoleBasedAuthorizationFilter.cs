using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskLogSystem.Models;

namespace TaskLogSystem.CustomFilters
{
    public class RoleBasedAuthorizationFilter : ActionFilterAttribute, IAuthorizationFilter
    {
        private readonly int[] _allowedRoleIds;

        public RoleBasedAuthorizationFilter(params int[] allowedRoles)
        {
            _allowedRoleIds = allowedRoles;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var user = (Employee)filterContext.HttpContext.Session["CurrentUser"];

            foreach (var allowedRoleId in _allowedRoleIds)
            {
                if (user.RoleID == allowedRoleId)
                {
                    return; // User has one of the allowed roles, so allow access
                }
            }

            // If the user's role does not match any of the allowed roles, deny access
            filterContext.Result = new HttpUnauthorizedResult(); // Or any other appropriate result
        }
    }
}

//[RoleBasedAuthorizationFilter("Director", "Manager")]