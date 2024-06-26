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
using EmailProcessor;
using static MongoDB.Driver.WriteConcern;

namespace EOS.Services
{
    public class MongoEmailService : MongoService   
    {
        public MongoDBSettings _mongoEmailService;
        
        private List<KeyValuePair<string, string>> _emails;

        
        public List<KeyValuePair<string, string>> Emails
        {
            get { return _emails; }
            set { _emails = value; }
        }

        public MongoEmailService(IOptions<MongoDBSettings> mongoEmailService) : base(mongoEmailService)
        {
            
            _mongoEmailService = mongoEmailService.Value;
        }


        public List<KeyValuePair<string, string>> GetEmailMetadata(string userEmail)
        {

            //Return all the generated emails in users account
            List<KeyValuePair<string, string>> _generatedEmails = GetEmails(userEmail);

            //For each of the emails in the account
            foreach (KeyValuePair<string, string> _generatedEmail in _generatedEmails)
            {
                //Get the value (email)
                string emailToSearch = _generatedEmail.Value;
                Debug.WriteLine(emailToSearch);
                //Search the Email Database
                string collection = findCollectionCode(emailToSearch);
                Debug.WriteLine(collection);
                var _emailCollection = _emailDatabase.GetCollection<Email>(collection);
                var emailData = _emailCollection.Find(x => x.email == emailToSearch).First();
                Debug.WriteLine(emailData);
                
                foreach (KeyValuePair<string, string> email in emailData.Metadata) 
                {
                    if (_generatedEmail.Key != null && email.Key != null)
                    {
                        Debug.WriteLine(email.Key);
                        Debug.WriteLine(_generatedEmail.Key);
                        _emails.Add(KeyValuePair.Create(_generatedEmail.Key, email.Key));
                    }
                    else
                    {
                        Debug.WriteLine("emailDocuments null");
                    }
                }

            }
            return _emails;


        }

        public string findCollectionCode(string generatedEmailToResolve)
        {
            string emailCode = generatedEmailToResolve.Remove(36);
            emailCode = emailCode.Substring(32);
            return emailCode;
        }

        public bool DeleteInboxEmail(string genEmail, string date, string subject)
        {

            // Define a filter to find the email by genEmail
            var filter = Builders<Email>.Filter.Eq("Email", genEmail);

            //Find collection that email is in
            string collection = findCollectionCode(genEmail);
            var _emailCollection = _emailDatabase.GetCollection<Email>(collection);

            // Find the email document
            var emailDocument = _emailCollection.Find(filter).FirstOrDefault();

            if (emailDocument != null)
            {
                int i = 0;
                foreach (KeyValuePair<string, string> email in emailDocument.Metadata)
                {
                    if (email.Key == date && email.Value == subject)
                    {
                        var inboxDel = Builders<Email>.Update.PullFilter(u => u.Metadata, m => m.Value == subject && m.Key == date);
                        var result = _emailCollection.UpdateOne(filter, Builders<Email>.Update.Combine(inboxDel));

                        if (result.ModifiedCount > 0)  
                            return true;
                    }
                    else
                    {
                        i++;
                    }
                }

            }

            return false;
        }


        public void FlipIsEnabled(string genEmail)
        {
            // Define a filter to find the email by genEmail
            var filter = Builders<Email>.Filter.Eq("Email", genEmail);

            //Find collection that email is in
            string collection = findCollectionCode(genEmail);
            var _emailCollection = _emailDatabase.GetCollection<Email>(collection);

            var currentDocument = _emailCollection.Find(filter).FirstOrDefault();
            if (currentDocument != null)
            {
                bool currentIsEnabled = currentDocument.IsEnabled;
                if (currentIsEnabled == true)
                {
                    var update = Builders<Email>.Update.Set("IsEnabled", false);

                    var updateResult = _emailCollection.UpdateOne(filter, update);

                    if (updateResult.ModifiedCount > 0)
                    {
                        Debug.WriteLine("Set to False");
                    }
                    else
                    {
                        Debug.WriteLine("Set to False FAILED");
                    }
                }
                else
                {
                    var update = Builders<Email>.Update.Set("IsEnabled", true);

                    var updateResult = _emailCollection.UpdateOne(filter, update);

                    if (updateResult.ModifiedCount > 0)
                    {
                        Debug.WriteLine("Set to True");
                    }
                    else
                    {
                        Debug.WriteLine("Set to True FAILED");
                    }
                }
            }
            else
            {
                Debug.WriteLine("Document Null");
            }
        }

        
    }

}