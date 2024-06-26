using EmailProcessor;
using EOS.Models;
using EOS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace EOS.Controllers
{
    public class ContactController : Controller
    {
        private readonly MongoService _mongoService;
        public ContactController(MongoService mongoService) { _mongoService = mongoService; }

        public IActionResult Contact()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");

/*                //Mongo Connection Uri
                MongoClient client = new MongoClient("mongodb+srv://app-auth:ASWA94ipgzYajw5i@cluster0.t6zxzom.mongodb.net/?retryWrites=true&w=majority");

                //Identity Database
                IMongoDatabase identityDatabase = client.GetDatabase("Identity");

                // Filter the database
                IMongoCollection<Users> userDB = identityDatabase.GetCollection<Users>("Users"); //Using base Users model*/

                Users currentUser = new Users();
                var filter = Builders<Users>.Filter.Eq(x => x.Email, userEmail);
                currentUser = _mongoService._userCollection.Find(filter).FirstOrDefault();
                    
                // Moving the filtered user to the ViewBag
                ViewBag.CurrentUser = currentUser;

                // Fixing Names
                ViewBag.CurrentUser.firstname = new CultureInfo("en-US").TextInfo.ToTitleCase(ViewBag.CurrentUser.firstname);
                ViewBag.CurrentUser.lastName = new CultureInfo("en-US").TextInfo.ToTitleCase(ViewBag.CurrentUser.lastName);
                ViewBag.CurrentUser.location = new CultureInfo("en-US").TextInfo.ToTitleCase(ViewBag.CurrentUser.location);

                return View();
            }
            else
            {
                return View();
            }
        }

        public IActionResult ContactSendRequest(ContactEmail contact)
        {
            // Tidying the incoming data
            if (contact.name != null) { contact.name = contact.name.Trim().ToLower(); }
            if (contact.email != null) { contact.email = contact.email.Trim().ToLower(); }
            if (contact.subject != null) { contact.subject = contact.subject.Trim().ToLower(); }
            if (contact.content != null) { contact.content = contact.content.Trim().ToLower(); }


            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");

                //Mongo Connection Uri
                MongoClient client = new MongoClient("mongodb+srv://app-auth:ASWA94ipgzYajw5i@cluster0.t6zxzom.mongodb.net/?retryWrites=true&w=majority");

                //Identity Database
                IMongoDatabase identityDatabase = client.GetDatabase("Identity");

                // Filter the database
                IMongoCollection<NewUser> userDB = identityDatabase.GetCollection<NewUser>("Users"); //Using base Users model
                NewUser currentUser = new NewUser();
                var filter = Builders<NewUser>.Filter.Eq(x => x.Email, userEmail);
                currentUser = userDB.Find(filter).FirstOrDefault();

                // Preparing the return ViewBag
                ViewBag.CurrentUser = currentUser;

                // Fixing Names
                ViewBag.CurrentUser.firstname = new CultureInfo("en-US").TextInfo.ToTitleCase(ViewBag.CurrentUser.firstname);
                ViewBag.CurrentUser.lastName = new CultureInfo("en-US").TextInfo.ToTitleCase(ViewBag.CurrentUser.lastName);
                ViewBag.CurrentUser.location = new CultureInfo("en-US").TextInfo.ToTitleCase(ViewBag.CurrentUser.location);

                // I don't want to do this but it is what makes it work...
                ModelState.Remove("name");
                ModelState.Remove("email");
                ModelState.Remove("location");
            }

                // Reseting the message that appears on the next page(s)
                string msg = "";

            // Used for debugging and seeing what is happening inside ModelState.IsValid
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            // Removing ID as it doesnt exist for the model yet
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                string collection = "";

                // Adding request to MongoDB
                //Mongo Connection Uri
                MongoClient client = new MongoClient("mongodb+srv://app-auth:ASWA94ipgzYajw5i@cluster0.t6zxzom.mongodb.net/?retryWrites=true&w=majority");

                //Identity Database
                IMongoDatabase contactDatabase = client.GetDatabase("Contact");

                if (HttpContext.Session.GetString("UserEmail") != null)
                {
                    collection = "RegisteredUser";
                }
                else
                {
                    collection = "Guest";
                }

                // Selecting the collection
                var contactCollection = contactDatabase.GetCollection<ContactEmail>(collection);

                // Adding Record
                // Commented out during testing to prevent unnessesary time and additions to Mongo
                contactCollection.InsertOne(contact);

                // Create confirmation message
                msg += "Thank You for Contacting Us <3";
                TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // We should never get here if Contact page validation works in the first place       
                // Create confirmation message
                msg += "Incorrect Information Received";
                TempData["msg"] = "<script>alert(</script>" + msg + "<script>);</script>";

                // Returns to Contact Page to correct errors
                return View("Contact");
            }
        }

    }
}
