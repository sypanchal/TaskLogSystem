using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using TaskLogSystem.Models;
using TaskLogSystem.CustomModels;
using TaskLogSystem.CustomFilters;
using System.Data.SqlClient;
using System.Data;

namespace TaskLogSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class HomeController : Controller
    {
        TaskLogSystemEntities _dbContext = new TaskLogSystemEntities();


        //private List<UserMenuPermission> GetUserMenuPermissions(int employeeId)
        //{
        //    var menuPermissions = new List<UserMenuPermission>();

        //    using (var command = _dbContext.Database.Connection.CreateCommand())
        //    {
        //        command.CommandType = CommandType.StoredProcedure;
        //        command.CommandText = "GetUserMenuPermissions";

        //        var employeeIdParam = new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId };
        //        command.Parameters.Add(employeeIdParam);

        //        _dbContext.Database.Connection.Open();
        //        using (var reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                var menuPermission = new UserMenuPermission
        //                {
        //                    MenuID = reader.GetInt32(reader.GetOrdinal("MenuID")),
        //                    MenuName = reader.GetString(reader.GetOrdinal("MenuName")),
        //                    ParentMenuID = reader.IsDBNull(reader.GetOrdinal("ParentMenuID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ParentMenuID")),
        //                    IconName = reader.IsDBNull(reader.GetOrdinal("IconName")) ? null : reader.GetString(reader.GetOrdinal("IconName")),
        //                    AccessName = reader.GetString(reader.GetOrdinal("AccessName"))
        //                };
        //                menuPermissions.Add(menuPermission);
        //            }
        //        }

        //        _dbContext.Database.Connection.Close();
        //    }

        //    return menuPermissions;
        //}

        public ActionResult Index()
        {
            Employee user = null;
            if (Session["CurrentUser"] != null)
            {
                user = (Employee)Session["CurrentUser"];
            }

            List<UserMenuPermission> menuPermissions = new List<UserMenuPermission>();
            if (user != null)
                menuPermissions = GetUserMenuPermissions(user.EmployeeID);

            var menuTree = BuildMenuTree(menuPermissions);

            return View(menuTree); // Pass menuTree instead of menuPermissions
        }


        private List<UserMenuPermission> GetUserMenuPermissions(int employeeId)
        {
            var menuPermissions = new List<UserMenuPermission>();

            using (var command = _dbContext.Database.Connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "GetUserMenuPermissions";

                var employeeIdParam = new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId };
                command.Parameters.Add(employeeIdParam);

                _dbContext.Database.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var menuPermission = new UserMenuPermission
                        {
                            MenuID = reader.GetInt32(reader.GetOrdinal("MenuID")),
                            MenuName = reader.GetString(reader.GetOrdinal("MenuName")),
                            ParentMenuID = reader.IsDBNull(reader.GetOrdinal("ParentMenuID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ParentMenuID")),
                            IconName = reader.IsDBNull(reader.GetOrdinal("IconName")) ? null : reader.GetString(reader.GetOrdinal("IconName")),
                            AccessName = reader.GetString(reader.GetOrdinal("AccessName"))
                        };
                        menuPermissions.Add(menuPermission);
                    }
                }

                _dbContext.Database.Connection.Close();
            }

            return menuPermissions;
        }

        private Dictionary<int?, List<UserMenuPermission>> BuildMenuTree(List<UserMenuPermission> menuPermissions)
        {
            var menuTree = new Dictionary<int?, List<UserMenuPermission>>();

            foreach (var menu in menuPermissions)
            {
                int? parentMenuID = menu.ParentMenuID; // Get the parent menu ID

                // Check if the parent menu ID is null
                if (parentMenuID == null)
                {
                    // If null, set it to a default value or handle accordingly
                    parentMenuID = 0; // For example, set it to 0
                }

                // Check if the parent menu ID exists in the dictionary
                if (!menuTree.ContainsKey(parentMenuID))
                {
                    // If not, add it to the dictionary with an empty list
                    menuTree[parentMenuID] = new List<UserMenuPermission>();
                }

                // Add the current menu to the list associated with its parent menu ID
                menuTree[parentMenuID].Add(menu);
            }

            return menuTree;
        }

    }
}