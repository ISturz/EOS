using EOS.Models;
using EOS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Win32;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MongoDB.Driver.Linq;

namespace EOS.Controllers
{
    public class LoginController : Controller
    {

        private readonly MongoAccountService _mongoAccountService;
        private readonly MongoService _mongoService;
        public LoginController(MongoAccountService mongoAccountService, MongoService mongoService)
        {
            _mongoAccountService = mongoAccountService;
            _mongoService = mongoService;
        }

        [HttpPost]
        public async Task<ActionResult> LoginAction(Users login)
        {
            // Reseting the message that appears on the next page(s)
            string msg = null;

            ModelState.Remove("GeneratedEmails");
            ModelState.Remove("DeletedEmails");
            ModelState.Remove("Id");
            ModelState.Remove("RememberMe");

            if (login != null)
            {
                login.Email = login.Email.ToLower();
            }
           
            Users user = new Users
            {
                Email = login.Email,
                Password = login.Password,
            };

            // Process if Model is Valid
            if (ModelState.IsValid)
            {
                
                foreach (var key in ModelState.Keys)
                {
                    var error = ModelState[key].Errors.FirstOrDefault();
                    if (error != null)
                    {
                        var errorMessage = error.ErrorMessage;
                        Debug.WriteLine(errorMessage);
                    }
                }

                // Checking the entered email against mongoDB entries
                bool emailExists = _mongoService.CheckUserExists(user.Email);

                if (emailExists)
                {
                    //Verify Password matches
                    bool passwordMatches = _mongoAccountService.VerifyPassword(user);
                    if (passwordMatches)
                    {
                        //Store user email in session
                        HttpContext.Session.SetString("UserEmail", user.Email);

                        if (login.RememberMe)
                        {
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, user.Email)
                                // Add more claims if needed
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            var authProperties = new AuthenticationProperties
                            {
                                IsPersistent = true, // This makes the cookie persistent
                                ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(30))
                            };
                            Debug.WriteLine("Remember me is functioning");

                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                        }
                        //msg += "Login is successfull";
                        TempData["msg"] = null;
                        // Return to Home Page once user is registered
                        return RedirectToAction("MyEmails", "MyEmails");
                    }
                    else
                    {
                        msg += "Incorrect  Password";
                        TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                        // Return to Registration Page to correct errors
                        return RedirectToAction("Index", "Home");
                    };
                }
                else
                {
                    msg += "Unknown Email or Password";
                    TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                    // Return to Registration Page to correct errors
                    return RedirectToAction("MyEmails", "MyEmails");
                };
            }
            else
            // Process if Model is invalid
            {
                // We should never get here if Registration page validation works in the first place       

                foreach (var entry in ModelState)
                {
                    var propertyName = entry.Key; // The name of the property with the error
                    var errorMessages = entry.Value.Errors.Select(e => e.ErrorMessage); // List of error messages for the property

                    foreach (var errorMessage in errorMessages)
                    {
                        // Log or debug the error message
                        Debug.WriteLine($"Validation error for property '{propertyName}': {errorMessage}");
                    }
                }

                // Create confirmation message
                msg += "Model State Invalid";
                TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                // Returns to Registration Page to correct errors
                return RedirectToAction("Index", "Home");
            }
        }


        public IActionResult Logout()
        {
            // Clear the user's session to log them out
            HttpContext.Session.Clear();

            // Redirect the user to the desired page after logout
            return RedirectToAction("Index", "Home"); // Redirect to the homepage
        }
    }
}
