using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using TaskLogSystem.Models;
using TaskLogSystem.CustomFilters;
using System.Collections;

namespace TaskLogSystem.Controllers
{
    public class DatatableController : Controller
    {
        private TaskLogSystemEntities _dbContext = new TaskLogSystemEntities();

        // POST: Datatable/LoadTasksData
        [HttpPost]
        [RoleBasedAuthorizationFilter(2, 3)]
        public JsonResult LoadTasksData()
        {
            Employee Emp = Session["CurrentUser"] as Employee;

            try
            {
                int totalRecord = 0;
                int filterRecord = 0;

                var draw = Request["draw"];

                var sortColumn = Request["columns[" + Request["order[0][column]"] + "][name]"];
                var sortColumnDirection = Request["order[0][dir]"];
                var searchValue = Request["search[value]"];

                var start = Request["start"];
                var length = Request["length"];

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all Task data for the current user (Employee)
                var list = _dbContext.Tasks.Where(t => t.EmployeeID == Emp.EmployeeID && !t.IsDeleted).ToList();

                // Now perform the formatting and additional processing in-memory
                var formattedList = list.Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.TaskDescription,
                    TaskDate = t.TaskDate.Value.ToString("dd MMM yyyy"),
                    ApproverName = t.Employee1.FirstName + " " + t.Employee1.LastName,
                    ApprovedorRejectedByName = t.ApprovedorRejectedOn.HasValue ? t.Employee.FirstName + " " + t.Employee.LastName : "Pending",
                    ApprovedorRejectedOn = t.ApprovedorRejectedOn.HasValue ? t.ApprovedorRejectedOn.Value.ToString("dd MMM yyyy hh:mm:ss") : "Pending",
                    t.Status,
                    CreatedOn = t.CreatedOn.HasValue ? t.CreatedOn.Value.ToString("dd MMM yyyy hh:mm:ss") : null,
                    ModifiedOn = t.ModifiedOn.HasValue ? t.ModifiedOn.Value.ToString("dd MMM yyyy hh:mm:ss") : null
                }).AsQueryable();

                // Total number of rows count
                totalRecord = formattedList.Count();

                // Search data when search value found
                if (!string.IsNullOrEmpty(searchValue))
                {
                    formattedList = formattedList.Where(x =>
                    x.TaskName.ToLower().Contains(searchValue.ToLower()) ||
                    x.TaskDescription.ToLower().Contains(searchValue.ToLower()) ||
                    x.ApproverName.ToLower().Contains(searchValue.ToLower()) ||
                    x.ApprovedorRejectedByName.ToLower().Contains(searchValue.ToLower()) ||
                    x.Status.ToLower().Contains(searchValue.ToLower()));
                }

                // Get total count of records after search
                filterRecord = formattedList.Count();

                // Sort data
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    formattedList = formattedList.OrderBy(sortColumn + " " + sortColumnDirection);
                else
                    formattedList = formattedList.OrderByDescending(t => t.ModifiedOn);

                // Pagination
                var TaskData = pageSize == -1 ? formattedList.ToList() : formattedList.Skip(skip).Take(pageSize).ToList();

                //Returning Json Data
                return Json(new { draw, recordsFiltered = filterRecord, recordsTotal = totalRecord, data = TaskData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception and return a generic error message
                //Log.Error("Error occurred while loading data", ex);
                return Json(new { draw = 0, recordsFiltered = 0, recordsTotal = 0, data = new List<Task>() });
            }
        }

        // POST: Datatable/LoadEmployeesData
        [HttpPost]
        [RoleBasedAuthorizationFilter(1, 2)]
        public JsonResult LoadEmployeesData()
        {
            Employee Emp = Session["CurrentUser"] as Employee;

            try
            {
                int totalRecord = 0;
                int filterRecord = 0;

                var draw = Request["draw"];

                var sortColumn = Request["columns[" + Request["order[0][column]"] + "][name]"];
                var sortColumnDirection = Request["order[0][dir]"];
                var searchValue = Request["search[value]"];

                var start = Request["start"];
                var length = Request["length"];

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all Employees data for the current user (Manager or Director)
                var list = Emp.DepartmentID == 2 ?
                    _dbContext.Employees.Where(t => t.ReportingPerson == Emp.EmployeeID && !t.IsDeleted).ToList()
                    : _dbContext.Employees.Where(t => !t.IsDeleted && t.ReportingPerson != null).ToList();

                // Now perform the formatting and additional processing in-memory
                var formattedList = list.Select(t => new
                {
                    t.EmployeeID,
                    t.EmployeeCode,
                    EmployeeName = t.FirstName + " " + t.LastName,
                    Gender = t.Gender == "M" ? "Male" : t.Gender == "F" ? "Female" : "Others",
                    t.Email,
                    t.Department.DepartmentName,
                    ReportingPersonName = t.Employee2.FirstName + " " + t.Employee2.LastName,
                }).AsQueryable();

                // Total number of rows count
                totalRecord = formattedList.Count();

                // Search data when search value found
                if (!string.IsNullOrEmpty(searchValue))
                {
                    formattedList = formattedList.Where(x =>
                    x.EmployeeCode.ToLower().Contains(searchValue.ToLower()) ||
                    x.EmployeeName.ToLower().Contains(searchValue.ToLower()) ||
                    x.Gender.ToLower().Contains(searchValue.ToLower()) ||
                    x.Email.ToLower().Contains(searchValue.ToLower()) ||
                    x.DepartmentName.ToLower().Contains(searchValue.ToLower()) ||
                    x.ReportingPersonName.ToLower().Contains(searchValue.ToLower()));
                }

                // Get total count of records after search
                filterRecord = formattedList.Count();

                // Sort data
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    var sortingExpression = sortColumn + " " + sortColumnDirection;
                    formattedList = formattedList.OrderBy(sortingExpression);
                }

                // Pagination
                var EmployeeData = pageSize == -1 ? formattedList.ToList() : formattedList.Skip(skip).Take(pageSize).ToList();

                //Returning Json Data
                return Json(new { draw, recordsFiltered = filterRecord, recordsTotal = totalRecord, data = EmployeeData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception and return a generic error message
                //Log.Error("Error occurred while loading data", ex);
                return Json(new { draw = 0, recordsFiltered = 0, recordsTotal = 0, data = new List<Employee>() });
            }
        }

        // POST: Datatable/LoadAllTasksData
        [HttpPost]
        [RoleBasedAuthorizationFilter(1, 2)]
        public JsonResult LoadAllTasksData()
        {
            Employee Emp = Session["CurrentUser"] as Employee;

            try
            {
                int totalRecord = 0;
                int filterRecord = 0;

                var draw = Request["draw"];

                var sortColumn = Request["columns[" + Request["order[0][column]"] + "][name]"];
                var sortColumnDirection = Request["order[0][dir]"];
                var searchValue = Request["search[value]"];

                var start = Request["start"];
                var length = Request["length"];

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all Employees data for the current user (Manager or Director)
                var list = Emp.DepartmentID == 2 ? _dbContext.Tasks.Where(t => t.ApproverID == Emp.EmployeeID && !t.IsDeleted).ToList() : _dbContext.Tasks.Where(t => !t.IsDeleted).ToList();

                // Now perform the formatting and additional processing in-memory
                var formattedList = list.Select(t => new
                {
                    t.Employee2.EmployeeCode,
                    EmployeeName = t.Employee2.FirstName + " " + t.Employee2.LastName,
                    t.TaskID,
                    t.TaskName,
                    t.TaskDescription,
                    TaskDate = t.TaskDate.Value.ToString("dd MMM yyyy"),
                    ApproverName = t.Employee1.FirstName + " " + t.Employee1.LastName,
                    ApprovedorRejectedByName = t.ApprovedorRejectedOn.HasValue ? t.Employee.FirstName + " " + t.Employee.LastName : "Pending",
                    ApprovedorRejectedOn = t.ApprovedorRejectedOn.HasValue ? t.ApprovedorRejectedOn.Value.ToString("dd MMM yyyy hh:mm:ss") : "Pending",
                    t.Status,
                    CreatedOn = t.CreatedOn.HasValue ? t.CreatedOn.Value.ToString("dd MMM yyyy hh:mm:ss") : null,
                    ModifiedOn = t.ModifiedOn.HasValue ? t.ModifiedOn.Value.ToString("dd MMM yyyy hh:mm:ss") : null
                }).AsQueryable();

                // Total number of rows count
                totalRecord = formattedList.Count();

                // Search data when search value found
                if (!string.IsNullOrEmpty(searchValue))
                {
                    formattedList = formattedList.Where(x =>
                    x.TaskName.ToLower().Contains(searchValue.ToLower()) ||
                    x.TaskDescription.ToLower().Contains(searchValue.ToLower()) ||
                    x.ApproverName.ToLower().Contains(searchValue.ToLower()) ||
                    x.ApprovedorRejectedByName.ToLower().Contains(searchValue.ToLower()) ||
                    x.Status.ToLower().Contains(searchValue.ToLower()) ||
                    x.EmployeeName.ToLower().Contains(searchValue.ToLower()) ||
                    x.EmployeeCode.ToLower().Contains(searchValue.ToLower())
                    );
                }

                // Get total count of records after search
                filterRecord = formattedList.Count();

                // Sort data
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    var sortingExpression = sortColumn + " " + sortColumnDirection;
                    formattedList = formattedList.OrderBy(sortingExpression);
                }

                // Pagination
                var TaskData = pageSize == -1 ? formattedList.ToList() : formattedList.Skip(skip).Take(pageSize).ToList();

                //Returning Json Data
                return Json(new { draw, recordsFiltered = filterRecord, recordsTotal = totalRecord, data = TaskData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception and return a generic error message
                //Log.Error("Error occurred while loading data", ex);
                return Json(new { draw = 0, recordsFiltered = 0, recordsTotal = 0, data = new List<Task>() });
            }
        }

        // POST: Datatable/LoadEmployeeTaskData/102
        [HttpPost]
        [RoleBasedAuthorizationFilter(1)]
        public JsonResult LoadEmployeeTaskData(int EmployeeID)
        {
            try
            {
                int totalRecord = 0;
                int filterRecord = 0;

                var draw = Request["draw"];

                var sortColumn = Request["columns[" + Request["order[0][column]"] + "][name]"];
                var sortColumnDirection = Request["order[0][dir]"];
                var searchValue = Request["search[value]"];

                var start = Request["start"];
                var length = Request["length"];

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all Employees data for the current user (Manager or Director)
                var list = _dbContext.Tasks.Where(t => t.EmployeeID == EmployeeID && !t.IsDeleted).ToList();

                // Now perform the formatting and additional processing in-memory
                var formattedList = list.Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.TaskDescription,
                    TaskDate = t.TaskDate.Value.ToString("dd MMM yyyy"),
                    ApproverName = t.Employee1.FirstName + " " + t.Employee1.LastName,
                    ApprovedorRejectedByName = t.ApprovedorRejectedOn.HasValue ? t.Employee.FirstName + " " + t.Employee.LastName : "Pending",
                    ApprovedorRejectedOn = t.ApprovedorRejectedOn.HasValue ? t.ApprovedorRejectedOn.Value.ToString("dd MMM yyyy hh:mm:ss") : "Pending",
                    t.Status,
                    CreatedOn = t.CreatedOn.HasValue ? t.CreatedOn.Value.ToString("dd MMM yyyy hh:mm:ss") : null,
                    ModifiedOn = t.ModifiedOn.HasValue ? t.ModifiedOn.Value.ToString("dd MMM yyyy hh:mm:ss") : null
                }).AsQueryable();

                // Total number of rows count
                totalRecord = formattedList.Count();

                // Search data when search value found
                if (!string.IsNullOrEmpty(searchValue))
                {
                    formattedList = formattedList.Where(x =>
                    x.TaskName.ToLower().Contains(searchValue.ToLower()) ||
                    x.TaskDescription.ToLower().Contains(searchValue.ToLower()) ||
                    x.ApproverName.ToLower().Contains(searchValue.ToLower()) ||
                    x.ApprovedorRejectedByName.ToLower().Contains(searchValue.ToLower()) ||
                    x.Status.ToLower().Contains(searchValue.ToLower())
                    );
                }

                // Get total count of records after search
                filterRecord = formattedList.Count();

                // Sort data
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    var sortingExpression = sortColumn + " " + sortColumnDirection;
                    formattedList = formattedList.OrderBy(sortingExpression);
                }

                // Pagination
                var TaskData = pageSize == -1 ? formattedList.ToList() : formattedList.Skip(skip).Take(pageSize).ToList();

                //Returning Json Data
                return Json(new { draw, recordsFiltered = filterRecord, recordsTotal = totalRecord, data = TaskData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception and return a generic error message
                //Log.Error("Error occurred while loading data", ex);
                return Json(new { draw = 0, recordsFiltered = 0, recordsTotal = 0, data = new List<Task>() });
            }
        }
    }
}