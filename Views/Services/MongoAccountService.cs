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
using System;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using OpenQA.Selenium.DevTools.V113.Emulation;

namespace EOS.Services
{

    public class MongoAccountService : MongoService
    {
        private MongoDBSettings _mongoAccountService;

        public MongoAccountService(IOptions<MongoDBSettings> mongoAccountSettings) : base(mongoAccountSettings)
        {
            _mongoAccountService = mongoAccountSettings.Value;
        }


        public bool DeleteAccount(string userEmail)
        {

            var filter = Builders<LoggedIn>.Filter.Eq(u => u.Email, userEmail);
            try
            {
                var result = _userCollectionAccount.DeleteOne(filter);

                if (result.DeletedCount > 0)
                {
                    return true; // Account was successfully deleted
                }
                else
                {
                    return false; // Account was not found or not deleted
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the deletion process
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }


        public LoggedIn GetUserByEmail(string userEmail)
        {
            try
            {
                var filter = Builders<LoggedIn>.Filter.Eq("Email", userEmail);
                var user = _userCollectionAccount.Find(filter).FirstOrDefault();
                return user;
            }
            catch (Exception ex)
            {
                // Log the exception here
                // You can use a logging library or simply Console.WriteLine for debugging
                Console.WriteLine($"An error occurred while retrieving user: {ex.Message}");
                return null;
            }
            return null;
        }

        public bool UpdateUser(string userEmail, LoggedIn updatedUser)
        {

            // Create a filter to find the user based on their email
            var filter = Builders<LoggedIn>.Filter.Eq(u => u.Email, userEmail);

            // Create an update definition with the changes you want to make
            var update = Builders<LoggedIn>.Update
                .Set(u => u.firstname, updatedUser.firstname)
                .Set(u => u.lastName, updatedUser.lastName)
                .Set(u => u.location, updatedUser.location)
                .Set(u => u.Password, updatedUser.Password);

            // Get a reference to the user collection and execute the update

            var updateResult = _userCollectionAccount.UpdateOne(filter, update);

            if (updateResult.IsAcknowledged && updateResult.ModifiedCount > 0)
            {
                // Update was successful
                return true;
            }
            else
            {
                // Update failed or no documents were modified
                return false;
            }
        }

        public bool DeleteGenEmailAddress(string genEmail, string genWebsite, string sessionEmail)
        {
            var filter = Builders<LoggedIn>.Filter.Eq(u => u.Email, sessionEmail);
            var pullUpdate = Builders<LoggedIn>.Update.PullFilter(u => u.GeneratedEmails, g => g.Value == genEmail);
            var pushUpdate = Builders<LoggedIn>.Update.Push(u => u.DeletedEmails, new KeyValuePair<string, string>(genWebsite, genEmail));

            try
            {
                var result = _userCollectionAccount.UpdateOne(filter, Builders<LoggedIn>.Update.Combine(pullUpdate, pushUpdate));

                if (result.ModifiedCount > 0)
                {

                    Debug.WriteLine(genEmail + " was deleted succesfully");
                    // Document with matching email in GeneratedEmails array was deleted.
                    return true;
                }
                else
                {
                    Debug.WriteLine(genEmail + " failed");
                    // No document with a matching email was found.
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the deletion process
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

        public bool ReactivateGenEmailAddress(string genEmail, string genWebsite, string sessionEmail)
        {
            var filter = Builders<LoggedIn>.Filter.Eq(u => u.Email, sessionEmail);
            var pullUpdate = Builders<LoggedIn>.Update.PullFilter(u => u.DeletedEmails, d => d.Value == genEmail);
            var pushUpdate = Builders<LoggedIn>.Update.Push(u => u.GeneratedEmails, new KeyValuePair<string, string>(genWebsite, genEmail));

            try
            {
                var result = _userCollectionAccount.UpdateOne(filter, Builders<LoggedIn>.Update.Combine(pullUpdate, pushUpdate));

                if (result.ModifiedCount > 0)
                {

                    Debug.WriteLine(genEmail + " was reactivated succesfully");
                    // Document with matching email in GeneratedEmails array was deleted.
                    return true;
                }
                else
                {
                    Debug.WriteLine(genEmail + " failed");
                    // No document with a matching email was found.
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the deletion process
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

        public bool EditGeneratedEmail(string websiteName, string genEmail, string sessionEmail)
        {
            var filter = Builders<LoggedIn>.Filter.And(
                Builders<LoggedIn>.Filter.Eq(u => u.Email, sessionEmail),
                Builders<LoggedIn>.Filter.ElemMatch(u => u.GeneratedEmails, g => g.Value == genEmail)
            );

            var update = Builders<LoggedIn>.Update.Set("GeneratedEmails.$.Key", websiteName);

            try
            {
                var result = _userCollectionAccount.UpdateOne(filter, update);

                if (result.ModifiedCount > 0)
                {
                    Debug.WriteLine(genEmail + " has a new website " + websiteName);
                    return true;
                }
                else
                {
                    Debug.WriteLine("Failed to update the website for " + genEmail);
                    return false;
                }


            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the deletion process
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

    }
}




    





















