using EOS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;

namespace EOS.Controllers
{
    public class DeletedEmailController : Controller
    {

        private readonly MongoAccountService _mongoAccountService;
        private readonly MongoGenerateService _mongoGenerateService;
        private readonly MongoEmailService _mongoEmailService;
        private ModelError error;

        public DeletedEmailController(MongoAccountService mongoAccountService, MongoGenerateService mongoGenerateService, MongoEmailService mongoEmailService)
        {
            _mongoAccountService = mongoAccountService;
            _mongoGenerateService = mongoGenerateService;
            _mongoEmailService = mongoEmailService;
        }
        public IActionResult DeletedEmails(string searchString)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Inbox", "Inbox");
            }

            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                var user = _mongoAccountService.GetUserByEmail(userEmail);

                if (user != null)
                {
                    var filteredEmails = new List<KeyValuePair<string, string>>();

                    if (!string.IsNullOrEmpty(searchString))
                    {
                        searchString = searchString.ToLower(); // Convert search string to lowercase for case-insensitive search

                        // Filter emails based on the search string
                        filteredEmails = user.DeletedEmails
                            .Where(email => email.Key.ToLower().Contains(searchString) || email.Value.ToLower().Contains(searchString))
                            .ToList();


                        user.DeletedEmails = filteredEmails;

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
            return RedirectToAction("DeletedEmails", "DeletedEmails");
        }

        public IActionResult ReActivateEmail(string genEmailAddress, string website) 
        {
            
            string sessionUserEmail = HttpContext.Session.GetString("UserEmail");

            if (_mongoAccountService.ReactivateGenEmailAddress(genEmailAddress, website, sessionUserEmail))
            {
                _mongoEmailService.FlipIsEnabled(genEmailAddress);
                return RedirectToAction("DeletedEmails", "DeletedEmail");
            }


            return RedirectToAction("Inbox", "Inbox");
        }
    }
}
