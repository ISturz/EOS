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



namespace EOS.Controllers
{
    public class RegisterController : Controller
    {
        private readonly MongoRegisterService _mongoRegisterService;
        private ModelError error;

        public RegisterController(MongoRegisterService mongoRegisterService)
        {
            _mongoRegisterService = mongoRegisterService;
        }

        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
                return RedirectToAction("Inbox", "Inbox");
            else
                return View();
        }

        [HttpPost]
        public async Task<ActionResult> RegisterAction(NewUser register)
        {
            if (register.Password != register.ConfirmPassword)
            {
                // Password and Confirm Password do not match, add a model validation error.
                ModelState.AddModelError("ConfirmPassword", "The password and confirm password do not match.");
                return RedirectToAction("Register", "Register");
            }

            // Trimming front and back whitespaces // And ToLower all data input
            if (register.firstname != null) { register.firstname = register.firstname.Trim().ToLower(); }
            if (register.lastName != null) { register.lastName = register.lastName.Trim().ToLower(); }
            if (register.Email != null) { register.Email = register.Email.Trim().ToLower(); }
            if (register.location != null) { register.location = register.location.Trim().ToLower(); }

            NewUser _newUser = new NewUser
            {
                firstname = register.firstname,
                lastName = register.lastName,
                Email = register.Email,
                location = register.location,
                Password = register.Password,
                GeneratedEmails = new List<KeyValuePair<string, string>>(),
                DeletedEmails = new List<KeyValuePair<string, string>>(),
                RememberMe = true
            };

            // Remvoing there two fields from being check by ModelState.IsValid
            ModelState.Remove("GeneratedEmails");
            ModelState.Remove("Id");
            ModelState.Remove("DeletedEmails");
            ModelState.Remove("RememberMe");

            // Reseting the message that appears on the next page(s)
            string msg = "";

            // Used for debugging and seeing what is happening inside ModelState.IsValid
            var errors = ModelState.Values.SelectMany(v => v.Errors);


            if(!ModelState.IsValid) 
            {
                // We should never get here if Registration page validation works in the first place       
                // Create confirmation message
                msg += "Incorrect Information Received";
                TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                // Returns to Registration Page to correct errors
                return View("Register");
            }

            bool emailExists = _mongoRegisterService.CheckUserExists(_newUser.Email);

            if(!emailExists)
            {
                // Why await? Just do the thing? Adding new user to mongoDB
                await _mongoRegisterService.CreateAsync(_newUser);
                // Create confirmation message
                msg += "Account has been created successfully";
                TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                // Return to Home Page once user is registered
                return RedirectToAction("Index", "Home");
            }

            // Creating unsuccessful message
            msg += "This email address has already been registered";
            TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
            // Return to Registration Page to correct errors
            return View("Register");
        }  

        [HttpPost]
        public async Task<ActionResult> LoginAction(Users login)
        {
            // Reseting the message that appears on the next page(s)
            string msg = "";

            ModelState.Remove("GeneratedEmails");
            ModelState.Remove("Id");

            if(login != null)
            {
                login.Email = login.Email.ToLower();
            }

            Users user = new Users
            {
                Email = login.Email,
                Password = login.Password,
            };

            // Used for debugging and seeing what is happening inside ModelState.IsValid
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            // Process if Model is Valid
            {
                // Checking the entered email against mongoDB entries
                bool emailExists = _mongoRegisterService.CheckUserExists(user.Email);

                if (emailExists)
                {
                    //Verify Password matches
                    bool passwordMatches = _mongoRegisterService.VerifyPassword(user);
                    if (passwordMatches)
                    {
                        //Store user email in session
                        HttpContext.Session.SetString("UserEmail", user.Email);
                        /*
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
                                // Set properties like expiration time, etc.
                                IsPersistent = true
                            };
                            Debug.WriteLine("Remember me is functioning");

                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                        }*/
                        //msg += "Login is successfull";
                        TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                        // Return to Home Page once user is registered
                        return RedirectToAction("Inbox", "Inbox");
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
                    msg += "Email Incorrect";
                    TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                    // Return to Registration Page to correct errors
                    return RedirectToAction("Index", "Home");
                };
            }
            else
            // Process if Model is invalid
            {
                // We should never get here if Registration page validation works in the first place       

                // Create confirmation message
                msg += "Enter a Valid Email";
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
//asp-action="RegisterAction" 