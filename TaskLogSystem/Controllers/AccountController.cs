using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TaskLogSystem.Models;
using NLog;
using System.Web.Helpers;
using static System.Collections.Specialized.BitVector32;
using TaskLogSystem.CustomFilters;


namespace TaskLogSystem.Controllers
{
    public class AccountController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        TaskLogSystemEntities _dbContext = new TaskLogSystemEntities();

        // GET: Account/Signup
        public ActionResult Signup()
        {
            return View();
        }

        // Helper method to hash the password
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // POST: Account/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(string firstname, string lastname, string dateofbirth, string gender, string email, string password, string confirmPassword, string mobile_number)
        {
            //Validate firstname and lastname using regex
            string namePattern = @"^[a-zA-Z]{2,50}$";
            if (!Regex.IsMatch(firstname, namePattern))
            {
                ModelState.AddModelError("FirstName", "Enter First Name only with Alphabets upto 50 characters");
            }

            if (!Regex.IsMatch(lastname, namePattern))
            {
                ModelState.AddModelError("LastName", "Enter Last Name only with Alphabets upto 50 characters");
            }

            // Validate password and confirmPassword
            if (password != confirmPassword)
            {
                ModelState.AddModelError("Password", "The password and confirm password do not match");
            }

            // Validate email using regex
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address");
            }

            // Check if the email already exists in the database
            if (_dbContext.Employees.Any(u => u.Email == email))
            {
                ModelState.AddModelError("Email", "Email already exists. Please Login.");
            }

            // Validate mobile number
            if (!Regex.IsMatch(mobile_number, @"^\d{10}$"))
            {
                ModelState.AddModelError("MobileNumber", "Please enter a valid 10-digit mobile number");
            }

            // Check if the mobileNumber already exists in the database
            if (_dbContext.Employees.Any(u => u.MobileNumber == mobile_number))
            {
                ModelState.AddModelError("MobileNumber", "MobileNumber already exists. Enter different...");
            }

            if (ModelState.IsValid)
            {
                // Save the user to the database
                try
                {
                    // Parse the date string and extract the date part
                    DateTime dob = DateTime.Parse(dateofbirth);

                    // Hash the password
                    string hashedPassword = HashPassword(password);

                    // Create a new employee object
                    var newEmp = new Employee
                    {
                        FirstName = firstname,
                        LastName = lastname,
                        DateOfBirth = dob.Date,
                        Gender = gender,
                        Email = email,
                        Password = hashedPassword,
                        MobileNumber = mobile_number,
                        DepartmentID = 3,
                        ReportingPerson = 101
                    };

                    // Add the employee to the database
                    _dbContext.Employees.Add(newEmp);
                    _dbContext.SaveChanges();

                    //Action Log in Logfile
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Logger.Info($"{firstname} {lastname} with email '{email}' has signed up at {timestamp}");

                    // Redirect to success page or login page
                    return RedirectToAction("Login", "Account");
                }
                catch (Exception ex)
                {
                    // Handle database or other errors
                    ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            // Retrieve the user from the database based on the provided email
            var user = _dbContext.Employees.SingleOrDefault(u => u.Email == email && !u.IsDeleted);

            if (user == null)
            {
                // If email not exists in the database
                ModelState.AddModelError("Email", "Email not exists. Please Sign up.");
                return View();
            }

            if (user != null)
            {
                // Compare the hashed password with the entered password
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    // Passwords match, login successful
                    // Start session and store user ID
                    Session["CurrentUser"] = user;

                    // Redirect to home page or dashboard
                    return RedirectToAction("Index", "Home");
                }
            }

            // Invalid email or password
            ModelState.AddModelError("Password", "Invalid email or password.");
            return View();
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var user = Session["CurrentUser"] as Employee;

            // Log user logout
            Logger.Info($"{user.FirstName} {user.LastName} with email {user.Email} has logged out at {timestamp}");

            // Clear the user's session
            Session.Clear();

            // Alternatively, you can invalidate the session
            // Session.Abandon();

            // Redirect the user to the login page
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            // Retrieve the user from the database based on the provided email
            var user = _dbContext.Employees.SingleOrDefault(u => u.Email == email && !u.IsDeleted);

            if (user == null)
            {
                // If email not exists in the database
                ModelState.AddModelError("Email", "Email not exists. Please Sign up.");
                return View();
            } else
            {
                Session["ResetPasswordRequest"] = true;
                Session["ResetPasswordRequestBy"] = user.Email;

                return RedirectToAction("ResetPassword", "Account");
            }
        }

        // GET: Account/ResetPassword
        [ResetPasswordAuthenticationFilter]
        public ActionResult ResetPassword()
        {
            return View();
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResetPasswordAuthenticationFilter]
        public ActionResult ResetPassword(string email, string password, string confirmPassword)
        {
            // Validate password and confirmPassword
            if (password != confirmPassword)
            {
                ModelState.AddModelError("Password", "The password and confirm password do not match");
            }

            if (ModelState.IsValid)
            {
                // Save the user password to the database
                try
                {
                    // Retrieve the user from the database based on the provided email
                    var user = _dbContext.Employees.SingleOrDefault(u => u.Email == email && !u.IsDeleted);

                    // Hash the password
                    string hashedPassword = HashPassword(password);

                    user.Password = hashedPassword;

                    _dbContext.SaveChanges();

                    //Action Log in Logfile
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Logger.Info($"{user.FirstName} {user.LastName} with email '{email}' has Reset Password at {timestamp}");

                    // Redirect to success page or login page
                    return RedirectToAction("Login", "Account");
                }
                catch (Exception ex)
                {
                    // Handle database or other errors
                    ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }
    }
}