using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using TaskLogSystem.CustomFilters;
using TaskLogSystem.Models;

namespace TaskLogSystem.Controllers
{
    [CustomAuthenticationFilter]
    public class EmployeesController : Controller
    {
        private TaskLogSystemEntities _dbContext = new TaskLogSystemEntities();

        // GET: Employees/Settings/5
        public ActionResult Settings(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = _dbContext.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Settings/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Settings(int EmployeeID, string FirstName, string LastName, string DateOfBirth, string Gender, string Email, string MobileNumber)
        {
            //Validate firstname and lastname using regex
            string namePattern = @"^[a-zA-Z]{2,50}$";
            if (!Regex.IsMatch(FirstName, namePattern))
            {
                ModelState.AddModelError("FirstName", "Enter First Name only with Alphabets upto 50 characters");
            }

            if (!Regex.IsMatch(LastName, namePattern))
            {
                ModelState.AddModelError("LastName", "Enter Last Name only with Alphabets upto 50 characters");
            }

            // Validate email using regex
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(Email, emailPattern))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address");
            }

            // Check if the email or mobileNumber already exists in the database
            if (_dbContext.Employees.Any(u => u.Email == Email && u.EmployeeID != EmployeeID && !u.IsDeleted))
            {
                ModelState.AddModelError("Email", "Email already exists. Enter different...");
            }

            // Validate mobile number
            if (!Regex.IsMatch(MobileNumber, @"^\d{10}$"))
            {
                ModelState.AddModelError("MobileNumber", "Please enter a valid 10-digit mobile number");
            }

            // Check if the mobileNumber already exists in the database
            if (_dbContext.Employees.Any(u => u.MobileNumber == MobileNumber && u.EmployeeID != EmployeeID && !u.IsDeleted))
            {
                ModelState.AddModelError("MobileNumber", "MobileNumber already exists. Enter different...");
            }

            Employee employee = new Employee();

            // Parse the date string and extract the date part
            DateTime dob = DateTime.Parse(DateOfBirth);

            // Find a employee object
            employee = _dbContext.Employees.Find(EmployeeID);

            employee.FirstName = FirstName;
            employee.LastName = LastName;
            employee.DateOfBirth = dob.Date;
            employee.Gender = Gender;
            employee.Email = Email;
            employee.MobileNumber = MobileNumber;

            if (ModelState.IsValid)
            {
                // Save the user to the database
                try
                {
                    // Add the employee to the database
                    _dbContext.SaveChanges();

                    return RedirectToAction("Settings", "Employees");
                }
                catch (Exception ex)
                {
                    // Handle database or other errors
                    ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View("Settings", employee);
        }


        // GET: Employees/ApproveTasks
        [RoleBasedAuthorizationFilter(1, 2)]
        public ActionResult ApproveTasks()
        {
            return View();
            //Employee Emp = Session["CurrentUser"] as Employee;


            //if (Emp.DepartmentID == 2)
            //{
            //    return View(_dbContext.Tasks.Where(e => !e.IsDeleted && e.ApproverID == Emp.EmployeeID).OrderBy(e => e.ModifiedOn).ToList());
            //}
            //else
            //{
            //    return View(_dbContext.Tasks.Where(e => !e.IsDeleted).OrderByDescending(e => e.ModifiedOn).ToList());
            //}
        }

        // Post: Employees/ApproveTask
        [HttpPost]
        [RoleBasedAuthorizationFilter(1, 2)]
        public ActionResult ApproveTasks(int id, string status)
        {
            Employee Emp = Session["CurrentUser"] as Employee;

            Task task = _dbContext.Tasks.Find(id);

            if (task == null)
            {
                return Json(new
                {
                    success = false,
                });
            }
            task.Status = status;
            task.ApprovedorRejectedBy = Emp.EmployeeID;
            task.ApprovedorRejectedOn = DateTime.Now;
            _dbContext.SaveChanges();

            return Json(new
            {
                success = true,
            });
        }

        // GET: Employees/ViewEmployees
        [RoleBasedAuthorizationFilter(2)]
        public ActionResult ViewEmployees(int id)
        {
            return View(_dbContext.Employees.Where(e => !e.IsDeleted && e.ReportingPerson == id).ToList());
        }

        // GET: Employees/Create
        [RoleBasedAuthorizationFilter(1)]
        public ActionResult Create()
        {
            return PartialView("_EmployeeForm", new Employee());
        }

        // GET: Employees/Edit/5
        [RoleBasedAuthorizationFilter(1)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = _dbContext.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return PartialView("_EmployeeForm", employee);
        }

        // POST: Employee/Delete/5
        [HttpPost]
        [RoleBasedAuthorizationFilter(1)]
        public ActionResult Delete(int id)
        {
            Employee employee = _dbContext.Employees.Find(id);
            employee.IsDeleted = true;
            //_dbContext.Tasks.Remove(task);
            _dbContext.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        // Helper method to hash the password
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Helper method to get the list of Ancestor Reporting Persons

        [RoleBasedAuthorizationFilter(1)]
        public ActionResult GetReportingPersonsListOnChange(int DepartmentID, int EmployeeID)
        {
            // Retrieve reporting persons list based on the selected department ID
            List<Employee> reportingPersonsList = EmployeeID != 0 ? _dbContext.Employees.Where(e => e.DepartmentID < DepartmentID && e.EmployeeID != EmployeeID && !e.IsDeleted).ToList() : _dbContext.Employees.Where(e => e.DepartmentID < DepartmentID && !e.IsDeleted).ToList();

            // Create a SelectList from the list of reporting persons
            var reportingPersonsSelectList = new SelectList(reportingPersonsList, "EmployeeID", "FirstName");

            // Return the SelectList as JSON
            return Json(reportingPersonsSelectList, JsonRequestBehavior.AllowGet);
        }

        //POST: Employees/CreateEmployee
        [HttpPost]
        [RoleBasedAuthorizationFilter(1)]
        public ActionResult CreateEmployee(Employee employee, string confirmPassword)
        {
            Employee ReportingEmployee = _dbContext.Employees.Find(employee.ReportingPerson);
            // Check if the departmentID of employee is lesser than reportingPerson's departmentID
            if (employee.DepartmentID < ReportingEmployee.DepartmentID)
            {
                ModelState.AddModelError("ReportingPerson", "ReportingPerson must have higher DepartmentID...");
            }

            //Validate firstname and lastname using regex
            string namePattern = @"^[a-zA-Z]{2,50}$";
            if (!Regex.IsMatch(employee.FirstName, namePattern))
            {
                ModelState.AddModelError("FirstName", "Enter First Name only with Alphabets upto 50 characters");
            }

            if (!Regex.IsMatch(employee.LastName, namePattern))
            {
                ModelState.AddModelError("LastName", "Enter Last Name only with Alphabets upto 50 characters");
            }

            // Validate password and confirmPassword
            if (employee.Password != confirmPassword)
            {
                ModelState.AddModelError("Password", "The password and confirm password do not match");
            }

            // Validate email using regex
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(employee.Email, emailPattern))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address");
            }

            // Check if the email or mobileNumber already exists in the database
            if (_dbContext.Employees.Any(u => u.Email == employee.Email))
            {
                ModelState.AddModelError("Email", "Email already exists. Enter different...");
            }

            // Validate mobile number
            if (!Regex.IsMatch(employee.MobileNumber, @"^\d{10}$"))
            {
                ModelState.AddModelError("MobileNumber", "Please enter a valid 10-digit mobile number");
            }

            // Check if the mobileNumber already exists in the database
            if (_dbContext.Employees.Any(u => u.MobileNumber == employee.MobileNumber))
            {
                ModelState.AddModelError("MobileNumber", "MobileNumber already exists. Enter different...");
            }

            if (ModelState.IsValid)
            {
                // Save the user to the database
                try
                {
                    // Parse the date string and extract the date part
                    DateTime dob = DateTime.Parse(employee.DateOfBirth.ToString());

                    // Hash the password
                    string hashedPassword = HashPassword(employee.Password);

                    // Create a new employee object
                    var empObj = new Employee();

                    empObj.FirstName = employee.FirstName;
                    empObj.LastName = employee.LastName;
                    empObj.DateOfBirth = dob.Date;
                    empObj.Gender = employee.Gender;
                    empObj.Email = employee.Email;
                    empObj.Password = hashedPassword;
                    empObj.MobileNumber = employee.MobileNumber;
                    empObj.DepartmentID = employee.DepartmentID;
                    empObj.ReportingPerson = employee.ReportingPerson;

                    // Add the employee to the database
                    _dbContext.Employees.Add(empObj);
                    _dbContext.SaveChanges();

                    return RedirectToAction("Index", "Home");

                }
                catch (Exception ex)
                {
                    // Handle database or other errors
                    ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                }
            }

            // If we got this far, something failed, redisplay form
            return PartialView("_EmployeeForm", employee);
        }

        //POST: Employees/UpdateEmployee
        [HttpPost]
        [RoleBasedAuthorizationFilter(1)]
        public ActionResult UpdateEmployee(int EmployeeID, string FirstName, string LastName, string DateOfBirth, string Gender, string Email, string MobileNumber, int DepartmentID, int ReportingPerson)
        {
            Employee ReportingEmployee = _dbContext.Employees.Find(ReportingPerson);
            // Check if the departmentID of employee is lesser than reportingPerson's departmentID
            if (DepartmentID < ReportingEmployee.DepartmentID)
            {
                ModelState.AddModelError("ReportingPerson", "ReportingPerson must have higher DepartmentID...");
            }

            //Validate firstname and lastname using regex
            string namePattern = @"^[a-zA-Z]{2,50}$";
            if (!Regex.IsMatch(FirstName, namePattern))
            {
                ModelState.AddModelError("FirstName", "Enter First Name only with Alphabets upto 50 characters");
            }

            if (!Regex.IsMatch(LastName, namePattern))
            {
                ModelState.AddModelError("LastName", "Enter Last Name only with Alphabets upto 50 characters");
            }

            // Validate email using regex
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(Email, emailPattern))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address");
            }

            // Check if the email or mobileNumber already exists in the database
            if (_dbContext.Employees.Any(u => u.Email == Email && u.EmployeeID != EmployeeID && !u.IsDeleted))
            {
                ModelState.AddModelError("Email", "Email already exists. Enter different...");
            }

            // Validate mobile number
            if (!Regex.IsMatch(MobileNumber, @"^\d{10}$"))
            {
                ModelState.AddModelError("MobileNumber", "Please enter a valid 10-digit mobile number");
            }

            // Check if the mobileNumber already exists in the database
            if (_dbContext.Employees.Any(u => u.MobileNumber == MobileNumber && u.EmployeeID != EmployeeID && !u.IsDeleted))
            {
                ModelState.AddModelError("MobileNumber", "MobileNumber already exists. Enter different...");
            }

            // Parse the date string and extract the date part
            DateTime dob = DateTime.Parse(DateOfBirth);

            // Find a new employee object
            Employee empObj = _dbContext.Employees.Find(EmployeeID);

            empObj.FirstName = FirstName;
            empObj.LastName = LastName;
            empObj.DateOfBirth = dob.Date;
            empObj.Gender = Gender;
            empObj.Email = Email;
            empObj.MobileNumber = MobileNumber;
            empObj.DepartmentID = DepartmentID;
            empObj.ReportingPerson = ReportingPerson;            

            if (ModelState.IsValid)
            {
                // Save the user to the database
                try
                {
                    // Add the employee to the database
                    _dbContext.SaveChanges();

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Handle database or other errors
                    ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                }
            }

            // If we got this far, something failed, redisplay form
            return PartialView("_EmployeeForm", empObj);
        }
    }
}
