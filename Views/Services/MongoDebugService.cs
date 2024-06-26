using EOS.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text;

using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using EOS.Controllers;
using MongoDB.Bson.IO;
using System.Diagnostics;


namespace EOS.Services
{
    public class MongoDebugService : MongoService
    {
        private MongoDBSettings _mongoDebugService;

        public MongoDebugService(IOptions<MongoDBSettings> mongoDebugSettings) : base(mongoDebugSettings) 
        {
            _mongoDebugService = mongoDebugSettings.Value;
        }

        //Here add functions to call from the debug cotroller
        //Check MongoServices for functions that are included in the services
        //Just put it in an async task//

        /*      You can now use these in the function or add a new one. This is controlled in the mongoService
         *      _userCollection 
                _userCollectionAccount 
                _emailCollection
                _emailDatabase*/

        public async Task CreateEmailAsync(string _user, string _account)
        {
            //Get the users generated emails
            Debug.WriteLine("Getting users generated emails");
            List<KeyValuePair<string, string>> _generatedEmails = GetEmails("hackenbergblake@gmail.com");

            //Define a filter to find the user
            var filter = Builders<Users>.Filter.Eq(u => u.Email, _user);

            //Take users email + id to gen a new email
            ObjectId userID = GetID(_user);
            string newEmail = generateEmail(_user, userID);

            //Create a new KeyPair with user input for Key (As the website) and Value as the new email
            _generatedEmails.Add(new KeyValuePair<string, string>(_account, newEmail + "@justadumbdomain.tech"));

            //Update the users generatedEmail list
            var update = Builders<Users>.Update.Set(u => u.GeneratedEmails, _generatedEmails);

            //Now lets add that to the user collection
            await _userCollection.FindOneAndUpdateAsync(filter, update);

            //First i need to find the database id?


            // Do I have to be a new model? hmmm Could i be a model extension of NewUser?
            // I will take the Email created. Add it to the email database as a document.
            // I will then take The users ID and attach it to the email document
            //Thinking i make a new class for the email database

            Email NewEmail = new Email
            {
                Id = new ObjectId(),
                email = newEmail,
                UserID = userID
            };

            // _emailCollection add email to collection
            // _emailCollection Add user Guid

            _emailCollection = _emailDatabase.GetCollection<Email>(Collection);
            _emailCollection.InsertOne(NewEmail);

        }

        /// <summary>
        /// Below are the functions that the above one uses to make good things happen
        /// </summary>

        //Takes the users email, returns their generated emails
        public List<KeyValuePair<string, string>> GetEmails(string email)
        {
            var _users = _userCollection.Find(x => x.Email == email).First();
            return _users.GeneratedEmails;
        }

        //Takes the users email, returns their ID (this will be changed)
        public ObjectId GetID(string email)
        {
            var _users = _userCollection.Find(x => x.Email == email).First();
            return _users.Id;
        }

        //Takes the users email, ID and the current time, hashes it with SHA256, then 
        //Turns that into an MD5 to shorten the length but keep the random
        public string generateEmail(string email, ObjectId id)
        {
            DateTime? created = DateTime.Now;
            string _fullString = email + id + created.ToString();

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array 
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(_fullString));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(builder.ToString());
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    return Convert.ToHexString(hashBytes).ToLower(); // .NET 5 +
                }
            }
        }
    }
}
