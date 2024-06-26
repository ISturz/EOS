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
    public class MongoGenerateService : MongoService
    {
        private MongoDBSettings _mongoGenerateService;
        
        public MongoGenerateService(IOptions<MongoDBSettings> mongoGenerateSettings) : base(mongoGenerateSettings)
        {
            _mongoGenerateService = mongoGenerateSettings.Value;
        }
        
        //Hello, I am the main function here that does all the important stuff
        public async Task CreateEmailAsync(string _user, string _account)
        {
            //Get the users generated emails
            Debug.WriteLine("Getting users generated emails");
            List<KeyValuePair<string, string>> _generatedEmails = GetEmails(_user);

            //Define a filter to find the user
            var filter = Builders<Users>.Filter.Eq(u => u.Email, _user);

            //Take users email + id to gen a new email
            ObjectId userID = GetID(_user);
            string newEmail = generateEmail(_user, userID);
            string emailCollection = CheckCollection(_emailDatabase);
            newEmail += emailCollection + "@justadumbdomain.tech";

            //Create a new KeyPair with user input for Key (As the website) and Value as the new email
            _generatedEmails.Add(new KeyValuePair<string, string>(_account, newEmail));

            //Update the users generatedEmail list
            var update = Builders<Users>.Update.Set(u => u.GeneratedEmails, _generatedEmails);
            //Now lets add that to the user collection
            await _userCollection.FindOneAndUpdateAsync(filter, update);
            //So Above adds the email to the users account, now we add to email database//
            //Create the model
            Email NewEmail = new Email
            {
                Id = new ObjectId(),
                email = newEmail,
                UserID = userID,
                Metadata = new List<KeyValuePair<string, string>>(),
                IsEnabled = true
            };

            // Find the collection // I'm in MongoService cause it works there better.
            var _emailCollection = _emailDatabase.GetCollection<Email>(emailCollection);
            _emailCollection.InsertOne(NewEmail);
        }

        //Takes the users email, returns their generated emails
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