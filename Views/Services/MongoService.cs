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
using OpenQA.Selenium;
using System.Collections.Generic;

namespace EOS.Services
{
    public class MongoService
    {
        //I am the mongo backend
        public string Collection = "";

        //This enables me to be used from any service
        public readonly IMongoCollection<Users> _userCollection;
        public readonly IMongoCollection<LoggedIn> _userCollectionAccount;

        public IMongoCollection<Email> _emailCollection;
        public readonly IMongoDatabase _emailDatabase;

        //public readonly IMongoCollection<Email> _userCollectionEmails;
        public List<KeyValuePair<string, string>> _emails;

        public MongoService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            //Mongo Connection Uri
            MongoClient client = new MongoClient("mongodb+srv://app-auth:ASWA94ipgzYajw5i@cluster0.t6zxzom.mongodb.net/?retryWrites=true&w=majority");
            
            //Identity Database
            IMongoDatabase identityDatabase = client.GetDatabase("Identity");
            IMongoDatabase emailDatabase = client.GetDatabase("Email");
            //Identity Collections
            _userCollection = identityDatabase.GetCollection<Users>("Users"); //Using base Users model
            _userCollectionAccount = identityDatabase.GetCollection<LoggedIn>("Users"); //Using NewUser Model

            //Email Database
            _emailDatabase = client.GetDatabase("Email");
        }

        //This is for repeated code that you may find across the services. 
        //Put the repeated code here, so you can access it from any service.
        
        //Takes the users email and returns a KVP List of their generated emails
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

        //Takes a generated email and retruns the collection its in
        public string findCollectionCode(string generatedEmailToResolve)
        {
            string emailCode = generatedEmailToResolve.Remove(36);
            emailCode = emailCode.Substring(32);
            return emailCode;
        }
        
        //Verifies the password used to log in with the users account
        public bool VerifyPassword(Users user)
        {
            var storedUser = _userCollection.Find(e => e.Email == user.Email).FirstOrDefault();
            if (storedUser != null)
            {
                string hashedInputPassword = encryptPassword(user.Password);
                return hashedInputPassword.Equals(storedUser.Password);
            }
            return false;
        }

        //Encrypts a new password with SHA256
        public string encryptPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array 
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }

        }

        //Checks to see if a users email already exists in the database. 
        /*public bool CheckUserExists(string newUserEmail)
        {
            Debug.WriteLine("Checking Email already exists");
            var dupe = _userCollectionAccount.Find(e => e.Email == newUserEmail).ToList();

            Debug.WriteLine("Checked for email and found: " + dupe);
            if (dupe.Count != 0)
            {
                Debug.WriteLine("Email already Exists: Returning True");
                return true;
            }
            else
            {
                Debug.WriteLine("Email Does not Exists: Returning False");
                return false;
            }
        }*/

        public bool CheckUserExists(string newUserEmail)
        {
            Debug.WriteLine("Checking Email already exists");
            if (newUserEmail.Length == 57)
            {
                string code = findCollectionCode(newUserEmail);
                IMongoCollection<Email> checkingCollection = _emailDatabase.GetCollection<Email>(code);
                if (checkingCollection.Find(x => x.email == newUserEmail) != null)
                {
                    Debug.WriteLine("Can not create new account with generated email");
                    return true;
                }
            }
            var dupe = _userCollectionAccount.Find(e => e.Email == newUserEmail).ToList();

            Debug.WriteLine("Checked for email and found: " + dupe);
            if (dupe.Count != 0)
            {
                Debug.WriteLine("Email already Exists: Returning True");
                return true;
            }
            else
            {
                Debug.WriteLine("Email Does not Exists: Returning False");
                return false;
            }
        }

        public List<Email> SearchDocumentsByEmail(List<string> valuesList)
        {
            List<Email> emailDocuments = new List<Email>();

            foreach (var document in valuesList) //Isaac had to hack this in, can you add error thingos
            {
                //Search the Email Database
                string collection = findCollectionCode(document); // Just edited redundancy out
                Debug.WriteLine("Searching collection: " + collection);
                var _userCollectionEmails = _emailDatabase.GetCollection<Email>(collection);
                var emaildata = _userCollectionEmails.Find(x => x.email == document).FirstOrDefault(); //Made a lambda expression, swapped valuesList for document

                Debug.WriteLine("Email data for document " + document + ": " + emaildata);

                if (emaildata != null)
                {
                    emailDocuments.Add(emaildata); //Instead of returning, we loop through and add 1 by 1
                    Debug.WriteLine("Added email to the list: " + emaildata.email);
                }
                else
                    Debug.WriteLine("No data found");
            }
           
            return emailDocuments;
        }

        //I have to be in MongoService cause who knows
        public string CheckCollection(IMongoDatabase emailDatabase)
        {
            //Create a new list to store the emailCollections
            List<string> emailCollections = new List<string>();
            //Get each collection in email database and add them to the list\\
            foreach (BsonDocument collection in emailDatabase.ListCollectionsAsync()
                    .Result.ToListAsync<BsonDocument>().Result)
            {
                string name = collection["name"].AsString;
                emailCollections.Add(name);
                Debug.WriteLine(collection["name"].ToString());
            }

            emailCollections.Sort();

            //There should be a better solution to this. But it works.
            foreach (var e in Enumerable.Reverse(emailCollections))
            {
                var _collection = _emailDatabase.GetCollection<Email>(e);
                var documents = _collection.CountDocuments(new BsonDocument());
                if (documents >= 5) //This is just to prove it works, Will be increased to 10,000 or 50,000 depends on scaling
                {

                    int docName = Convert.ToInt32(e, 16);
                    docName += 1;
                    emailDatabase.CreateCollection(docName.ToString("X4"));
                    Collection = docName.ToString("X4");
                    break;
                }
                else
                {
                    Collection = e;
                    break;
                }
            }
            return Collection;
        }

        //public List<Email> GetEmailsByUser(string userID)
        //{
        //    if (_userCollectionEmails == null)
        //    {
        //        throw new ArgumentNullException(nameof(_userCollectionEmails), "MongoDB collection is not initialized.");
        //    }
        //    try
        //    {
        //        var filter = Builders<Email>.Filter.Eq(x => x.Id, userID);
        //        if (filter == null)
        //        {
        //            throw new ArgumentNullException(nameof(filter), "Filter is null.");
        //        }
        //        var matchingEmails = _userCollectionEmails.Find(filter).ToList();
        //        foreach (var v in matchingEmails)
        //        {
        //            Debug.WriteLine(v.Id.ToString());
        //        }
        //        return matchingEmails;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception here
        //        // You can use a logging library or simply Console.WriteLine for debugging
        //        Debug.WriteLine($"An error occurred while retrieving user: {ex.Message}");
        //        return null;
        //    }
        //}

    }


}



