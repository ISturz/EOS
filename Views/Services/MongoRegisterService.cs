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

namespace EOS.Services
{

    public class MongoRegisterService : MongoService
    {
        private MongoDBSettings _mongoRegisterService;

        public MongoRegisterService(IOptions<MongoDBSettings> mongoRegisterSettings) : base(mongoRegisterSettings)
        {
            _mongoRegisterService = mongoRegisterSettings.Value;
        }

        public async Task CreateAsync(Users user) {
            user.Password = encryptPassword(user.Password);
            user.Id = new ObjectId();
            await _userCollection.InsertOneAsync(user);
            return;
        }

        public bool CheckUserLoggedIn(HttpContext context)
        {
            if (context.Session.GetString("UserEmail") != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DeleteAccount(string userEmail)
        {
            
            var filter = Builders<Users>.Filter.Eq(u => u.Email, userEmail);

            try
            {
                var result = _userCollection.DeleteOne(filter);

                if (result.DeletedCount > 0)
                {
                    return true; // Account was successfully deleted
                }
                else
                {
                    return false; // Account was not found or not deleted
                }
            }
            catch(Exception ex){
                // Handle any exceptions that might occur during the deletion process
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
    }
}