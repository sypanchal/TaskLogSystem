using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using TaskLogSystem.Models;
using TaskLogSystem.CustomFilters;

namespace TaskLogSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class HomeController : Controller
    {
        TaskLogSystemEntities _dbContext = new TaskLogSystemEntities();

        public ActionResult Index()
        {
            Employee Emp = Session["CurrentUser"] as Employee;

            if (Emp.DepartmentID == 1)
            {
                return View(_dbContext.Employees.Where(e => !e.IsDeleted && e.ReportingPerson != null).ToList());
            }
            else
            {
                return View(_dbContext.Tasks.Where(a => !a.IsDeleted && a.EmployeeID == Emp.EmployeeID).ToList());
            }
        }        
    }
}