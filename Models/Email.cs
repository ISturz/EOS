using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

//This is for the Email Database
namespace EOS.Models
{
    public class Email
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Email")]
        public string email { get; set; }

        [BsonElement("UserID")]
        public ObjectId UserID { get; set; }

        [BsonElement("Metadata")]
        [AllowNull]
        [StringLength(50)]
        public List<KeyValuePair<string, string>> Metadata { get; set; }

        public bool IsEnabled { get; set; }
    }


    public class InboxViewModel : Email
    {
        public List<KeyValuePair<string, string>> GeneratedEmailList { get; set; }
        public List<Email> InboxEmailList { get; set; }
    }
}