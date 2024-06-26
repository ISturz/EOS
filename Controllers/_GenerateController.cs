using EOS.Models;
using EOS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;

namespace EOS.Controllers
{
    public class GenerateController : Controller
    {

        private readonly MongoGenerateService _mongoGenerateService;
        private ModelError error;

        public GenerateController(MongoGenerateService mongoGenerateService)
        {
            _mongoGenerateService = mongoGenerateService;
        }

        public IActionResult Generate()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
                return View();
            else
                return RedirectToAction("Index", "Home");
        }

        //Get the List of generated emails
        [HttpPost]
        public async Task<ActionResult> GenerateAction(NewGeneratedEmail _email)
        {
   
            if (HttpContext.Session.GetString("UserEmail") == null) //This could be exploitable. 
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
    }
}
        