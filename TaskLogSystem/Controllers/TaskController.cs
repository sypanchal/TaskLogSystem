using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using TaskLogSystem.Models;
using TaskLogSystem.CustomFilters;

namespace TaskLogSystem.Controllers
{
    public class TaskController : Controller
    {
        private TaskLogSystemEntities _dbContext = new TaskLogSystemEntities();

        // GET: Task/Create
        public ActionResult Create()
        {
            return PartialView("_TaskLogForm", new Task());
        }

        // GET: Task/View/5
        public ActionResult View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Task task = _dbContext.Tasks.Find(id);
            if (task == null)
            {
                return HttpNotFound();
            }
            return PartialView("_TaskLogForm", task);
        }

        // GET: Task/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Task task = _dbContext.Tasks.Find(id);
            if (task == null)
            {
                return HttpNotFound();
            }
            return PartialView("_TaskLogForm", task);
        }

        // POST: Task/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpsertTask(Task Task)
        {
            Employee Emp = Session["CurrentUser"] as Employee;

            //Validate TaskName using regex
            string namePattern = @"^[a-zA-Z ]{2,50}$";
            if (!Regex.IsMatch(Task.TaskName, namePattern))
            {
                ModelState.AddModelError("TaskName", "Enter Task Name only with Alphabets upto 50 characters");
            }

            if (ModelState.IsValid)
            {
                // Save the task to the database
                try
                {
                    // Parse the date string and extract the date part
                    DateTime taskdate = DateTime.Parse(Task.TaskDate.ToString());

                    Task taskObj = new Task();

                    if (Task.TaskID == 0)
                    {
                        // Create a new task object
                        taskObj.EmployeeID = Emp.EmployeeID;
                        taskObj.TaskName = Task.TaskName;
                        taskObj.TaskDate = taskdate.Date;
                        taskObj.TaskDescription = Task.TaskDescription;
                        taskObj.ApproverID = Emp.ReportingPerson;
                        taskObj.Status = "Pending";
                        taskObj.CreatedOn = DateTime.Now;
                        taskObj.ModifiedOn = DateTime.Now;

                    } else
                    {
                        taskObj = _dbContext.Tasks.Find(Task.TaskID);
                      
                        taskObj.TaskName = Task.TaskName;
                        taskObj.TaskDate = taskdate.Date;
                        taskObj.TaskDescription = Task.TaskDescription;
                        taskObj.ModifiedOn = DateTime.Now;
                    }
                   
                    // Add the taskObj to the database
                    _dbContext.Tasks.AddOrUpdate(taskObj);
                    _dbContext.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        redirectUrl = Url.Action("Index", "Home")
                    });
                }
                catch (Exception ex)
                {
                    // Handle database or other errors
                    ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                }
            }

            // If we got this far, something failed, redisplay form with Errors
            return PartialView("_TaskLogForm", Task);
        }           

        // POST: Task/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Task task = _dbContext.Tasks.Find(id);
            task.IsDeleted = true;
            //_dbContext.Tasks.Remove(task);
            _dbContext.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
    }
}
