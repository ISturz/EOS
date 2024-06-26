using EOS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using EOS.Services;
using System.Diagnostics;
using Microsoft.Win32;
using Microsoft.VisualBasic;

namespace EOS.Controllers
{
    public class AccountController : Controller
    {

        private readonly MongoAccountService _mongoDBService;

        public AccountController(MongoAccountService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }


        public IActionResult Account()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                LoggedIn user = _mongoDBService.GetUserByEmail(userEmail);
                return View(user);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    

        public IActionResult AccountEdit(LoggedIn loggedInUser)
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                LoggedIn user = _mongoDBService.GetUserByEmail(userEmail);
                return View(user);
            }
            else
                return RedirectToAction("Index", "Home");
        }

        public IActionResult AccountEditConfirm(NewUser loggedInUser)
        {
            if (loggedInUser.firstname != null) { loggedInUser.firstname = loggedInUser.firstname.Trim().ToLower(); }
            if (loggedInUser.lastName != null) { loggedInUser.lastName = loggedInUser.lastName.Trim().ToLower(); }
            if (loggedInUser.Email != null) { loggedInUser.Email = loggedInUser.Email.Trim().ToLower(); }

            if (loggedInUser.Password != loggedInUser.ConfirmPassword)
            {
                // Password and Confirm Password do not match, add a model validation error.
                ModelState.AddModelError("ConfirmPassword", "The password and confirm password do not match.");
                return RedirectToAction("AccountEdit", "Account");
            }

            ModelState.Remove("Email");
            ModelState.Remove("GeneratedEmails");
            ModelState.Remove("DeletedEmails");
            if (!ModelState.IsValid)
            {
                // We should never get here if Registration page validation works in the first place       
                // Create confirmation message
                string msg = "Incorrect Information Received";
                TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                // Returns to Registration Page to correct errors
                return View("AccountEdit");
            }

            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                // Trimming front and back whitespaces // And ToLower all data input
                if (loggedInUser.firstname != null) { loggedInUser.firstname = loggedInUser.firstname.Trim().ToLower(); }
                if (loggedInUser.lastName != null) { loggedInUser.lastName = loggedInUser.lastName.Trim().ToLower(); }
                //if (loggedInUser.Email != null) { loggedInUser.Email = loggedInUser.Email.Trim().ToLower(); }
                if (loggedInUser.location != null) { loggedInUser.location = loggedInUser.location.Trim().ToLower(); }

                loggedInUser.Password = _mongoDBService.encryptPassword(loggedInUser.Password);

                LoggedIn UpdatedUser = new LoggedIn
                {
                    firstname = loggedInUser.firstname,
                    lastName = loggedInUser.lastName,
                    Password = loggedInUser.Password,
                    location = loggedInUser.location
                };

                string userEmail = HttpContext.Session.GetString("UserEmail");
                if (_mongoDBService.UpdateUser(userEmail, UpdatedUser))
                {
                    Debug.WriteLine("USER SUCCESSFULLY UPDATED");
                    return RedirectToAction("Account", "Account");

                }
                else
                {
                    Debug.WriteLine("USER NOT UPDATED");
                    return RedirectToAction("AccountEdit", "Account");
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccountDelete()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string msg = "";
                string UserEmail = HttpContext.Session.GetString("UserEmail").ToString();
                if (_mongoDBService.DeleteAccount(UserEmail))
                {
                    HttpContext.Session.Clear(); // Clear session after account deletion

                    msg += "This account has been deleted";
                    TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Handle case when MongoDB service is not available or account deletion fails
                    msg += "Not sure what happened, please try again";
                    TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                    return RedirectToAction("Account", "Account");
                }
            }

            return RedirectToAction("Index", "Home"); // Handle case when user is not logged in
        }
    }
}
