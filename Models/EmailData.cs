using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailProcessor
{
    public class emailData
    {
        // #Variables

        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("sentFrom")]
        public string sentFrom { get; set; }

        [BsonElement("sentFromName")]
        public string sentFromName { get; set; }

        [BsonElement("sentToName")]
        public string sentToName { get; set; }

        [BsonElement("sentTo")]
        public string sentTo { get; set; }

        [BsonElement("subjectLine")]
        public string subjectLine { get; set; }

        [BsonElement("dateReceived")]

        public DateTime dateReceived { get; set; }

        public string primaryEmail { get; set; }

        // Constructors

        // Default Constructor for emailData
        public emailData()
        {
            this.sentFrom = String.Empty;
            this.sentFromName = String.Empty;
            this.sentTo = String.Empty;
            this.sentToName = string.Empty;
            this.subjectLine = String.Empty;
            this.primaryEmail = String.Empty;
            this.dateReceived = DateTime.MinValue;
        }

        // Constructor
        public emailData(int ID, string sentFrom, string sentTo, string sentFromName, string sentToName, string subjectLine, string primaryEmail, DateTime dateReceived)
        {
            this.sentFrom = sentFrom;
            this.sentFromName = sentFromName;
            this.sentTo = sentTo;
            this.sentToName = sentToName;
            this.subjectLine = subjectLine;
            this.primaryEmail = primaryEmail;
            this.dateReceived = DateTime.MinValue;
        }

        // Methods

        public void Reset()
        {
            this.sentFrom = String.Empty;
            this.sentFromName = String.Empty;
            this.sentTo = String.Empty;
            this.sentToName = string.Empty;
            this.subjectLine = String.Empty;
            this.primaryEmail = String.Empty;
            this.dateReceived = DateTime.MinValue;
        }

        // For getting email address when there is a name and an address
        public string GetTo(string x)
        {
            int pFrom = x.IndexOf("<") + "<".Length;
            int pTo = x.LastIndexOf(">");
            return x.Substring(pFrom, pTo - pFrom);
        }

        // For getting email address when there is only an address
        public string GetToTwo(string x)
        {
            int pFrom = x.IndexOf(":") + ":".Length;
            return x.Substring(pFrom).Trim();
        }

        // For getting the name when there is a name and address
        public string GetName(string x)
        {
            int pFrom = x.IndexOf(":") + ":".Length;
            int pTo = x.LastIndexOf("<") + "<".Length;
            return x.Substring(pFrom, pTo - pFrom - 1).Trim();
        }
    }
}
