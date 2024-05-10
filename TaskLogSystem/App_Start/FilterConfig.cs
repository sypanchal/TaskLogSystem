using System.Web;
using System.Web.Mvc;

namespace TaskLogSystem
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ActionLogFilter());
        }
    }
}
