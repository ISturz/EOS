using EOS.Models;
using EOS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EOS.Controllers
{
    public class InboxController : Controller
    {

        private readonly MongoEmailService _mongoEmailService;
        private readonly MongoService _mongoService;

        private ModelError error;

        public InboxController(MongoEmailService mongoEmailService, MongoService mongoService)
        {
            
            _mongoService = mongoService;
            _mongoEmailService = mongoEmailService;

        }
        //public IActionResult Inbox()
        //{
        //    if (HttpContext.Session.GetString("UserEmail") != null)
        //    {
        //        string userEmail = HttpContext.Session.GetString("UserEmail");
        //        //string userID = _mongoService.GetID(userEmail);

        //        //List<KeyValuePair<string, string>> inboxList = new List<KeyValuePair<string, string>>();

        //        //inboxList = _mongoEmailService.GetEmailMetadata(userEmail);

        //        //foreach(var item in inboxList)
        //        //{
        //        //    Debug.WriteLine(item.Key, item.Value);
        //        //}

        //        var generatedEmailsList = _mongoService.GetEmails(userEmail); //This pulls list of generated emails

        //        List<string> valuesList = new List<string>(); 

        //        // Loop through the generatedEmailsList and extract the "value" fields
        //        foreach (var keyPairValue in generatedEmailsList)
        //        {
        //            valuesList.Add(keyPairValue.Value);                    
        //        }

        //        var inboxEmailList = _mongoService.SearchDocumentsByEmail(valuesList); //This calls function to get list of Inbox Emails

        //        var inboxViewModel = new InboxViewModel
        //        {
        //            GeneratedEmailList = generatedEmailsList,
        //            InboxEmailList = inboxEmailList
        //        };


        //        return View(inboxViewModel);
        //    }

        //    else
        //        return RedirectToAction("Index", "Home");
        //}

        public IActionResult RemoveInboxEmail(string genEmail, string date, string subject)
        {
            //Delete Email
            Debug.WriteLine("GeneratedEmail: " + genEmail);
            Debug.WriteLine("Date: " + date);
            Debug.WriteLine("subject: " + subject);

            if(_mongoEmailService.DeleteInboxEmail(genEmail, date, subject))
                return Redirect("Inbox");
            else
                return RedirectToAction("Index", "Home");
        }


        public IActionResult Inbox(string searchString)
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");

                //If Search bar is not Empty then it runs this
                if (!string.IsNullOrEmpty(searchString))
                {
                    Debug.WriteLine("Search String: " + searchString);

                    var generatedEmailsList = _mongoService.GetEmails(userEmail); //This pulls list of generated emails
                    List<string> valuesList = new List<string>();

                    // Filter generated emails based on the search string
                    var filteredInbox = new List<KeyValuePair<string, string>>();

                    searchString = searchString.ToLower(); // Convert search string to lowercase for case-insensitive search

                    // Filter emails based on the search string
                    filteredInbox = generatedEmailsList
                        .Where(email => email.Key.ToLower().Contains(searchString))
                        .ToList();

                    // Loop through the generatedEmailsList and extract the "value" fields
                    foreach (var keyPairValue in filteredInbox)
                    {
                        valuesList.Add(keyPairValue.Value);
                        Debug.WriteLine("Generated Email Value " + keyPairValue.Value);
                    }

                    var inboxEmailList = _mongoService.SearchDocumentsByEmail(valuesList); //This calls function to get list of Inbox Emails
                    
                    foreach(var item in inboxEmailList)
                    {
                        Debug.WriteLine("EMAIL METADATA " + item.Metadata);
                    }

                    var inboxViewModelSearch = new InboxViewModel
                    {
                        GeneratedEmailList = generatedEmailsList,
                        InboxEmailList = inboxEmailList 
                    };

                    Debug.WriteLine("COUNTS");
                    Debug.WriteLine("Filtered Inbox Email Count: " + inboxEmailList.Count);
                    Debug.WriteLine("Generated Email List Count: " + generatedEmailsList.Count);

                    foreach (var item in inboxViewModelSearch.InboxEmailList)
                    {
                        Debug.WriteLine("MODEL OUTPUT: " + item.email);
                    }

                    Debug.WriteLine(inboxViewModelSearch.GeneratedEmailList.Count);

                    return View(inboxViewModelSearch);
                }
                //If Search Bar is empty is runs this 
                else
                {
                    var generatedEmailsList = _mongoService.GetEmails(userEmail); //This pulls list of generated emails

                    List<string> valuesList = new List<string>();

                    // Loop through the generatedEmailsList and extract the "value" fields
                    foreach (var keyPairValue in generatedEmailsList)
                    {
                        valuesList.Add(keyPairValue.Value);
                        Debug.WriteLine("Generated Email Value " + keyPairValue.Value);
                    }

                    var inboxEmailList = _mongoService.SearchDocumentsByEmail(valuesList); //This calls function to get list of Inbox Emails

                    var inboxViewModel = new InboxViewModel
                    {
                        GeneratedEmailList = generatedEmailsList,
                        InboxEmailList = inboxEmailList
                    };

                    return View(inboxViewModel);
                }
            }

            else
                return RedirectToAction("Index", "Home");
        }
    }
}