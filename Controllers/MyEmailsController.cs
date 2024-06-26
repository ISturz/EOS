
using EOS.Models;
using EOS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using OpenQA.Selenium;
using System.Diagnostics;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Linq;

namespace EOS.Controllers
{
    public class MyEmailsController : Controller
    {
        private readonly MongoAccountService _mongoAccountService;
        private readonly MongoGenerateService _mongoGenerateService;
        private readonly MongoEmailService _mongoEmailService;
        private ModelError error;

        public MyEmailsController(MongoAccountService mongoAccountService, MongoGenerateService mongoGenerateService, MongoEmailService mongoEmailService)
        {
            _mongoAccountService = mongoAccountService;
            _mongoGenerateService = mongoGenerateService;
            _mongoEmailService = mongoEmailService;
        }

        public IActionResult MyEmails(string searchString)
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                var user = _mongoAccountService.GetUserByEmail(userEmail);

                if (user != null)
                {
                    Debug.WriteLine("This is the user if it's Not null " + user);

                    // Filter generated emails based on the search string
                    var filteredEmails = new List<KeyValuePair<string, string>>();

                    if (!string.IsNullOrEmpty(searchString))
                    {
                        searchString = searchString.ToLower(); // Convert search string to lowercase for case-insensitive search

                        // Filter emails based on the search string
                        filteredEmails = user.GeneratedEmails
                            .Where(email => email.Key.ToLower().Contains(searchString) || email.Value.ToLower().Contains(searchString))
                            .ToList();


                        user.GeneratedEmails = filteredEmails;

                        return View(user);
                    }
                    else
                    {
                        // If search string is empty, show all generated emails
                        Debug.WriteLine("Search String is empty, returning all");
                        return View(user);
                    }
                }
                else
                {
                    Debug.WriteLine("This is the user if its null " + user);
                }
            }
            return RedirectToAction("Inbox", "Inbox");
        }

        [HttpPost]
        public async Task<ActionResult> GenerateAction(NewGeneratedEmail _email)
        {
            Debug.WriteLine("New Accounnt = " + _email.Account);

            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                string msg = "";
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                await _mongoGenerateService.CreateEmailAsync(userEmail, _email.Account);


                return RedirectToAction("MyEmails", "MyEmails");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        //This returns the view
        public IActionResult EditEmailView(string Website, string genEmailAddress)
        {
            if(Website != null)
            {
                if (HttpContext.Session.GetString("UserEmail") != null)
                {
                    var model = new EditEmailModel
                    {
                        Website = Website,
                        GenEmail = genEmailAddress
                    };
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("MyEmails", "MyEmails");
        }

        //This is the action result of the EditEmailView 
        public IActionResult EditEmailAction(string newWebName, string genEmailAddress)
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");


                if (_mongoAccountService.EditGeneratedEmail(newWebName, genEmailAddress, userEmail))
                {
                    //Return to MyEmails page to see of change has been made
                    return RedirectToAction("MyEmails", "MyEmails");
                }

                //Stays on page if update was a fail
                return RedirectToAction("EditEmailView");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        public IActionResult RemoveGenEmail(string website, string genEmailAddress)
        {
            Debug.WriteLine("This is the email " + genEmailAddress + " for " + website);
            string sessionUserEmail = HttpContext.Session.GetString("UserEmail");
            string msg = null;

            if (_mongoAccountService.DeleteGenEmailAddress(genEmailAddress, website, sessionUserEmail))
            {
                _mongoEmailService.FlipIsEnabled(genEmailAddress);
                msg += "Email Deleted";
                TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";

                return RedirectToAction("MyEmails", "MyEmails");
            }


            return RedirectToAction("Inbox", "Inbox");
        }

    }
}
