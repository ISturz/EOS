using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EOS.Models
{
    public class ContactEmail
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Name")]
        [Required(ErrorMessage = "Please Enter an Name")]
        [AllowNull]
        [StringLength(35, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 35 characters")]
        [RegularExpression(@"^[^-][A-Za-z\s-]*", ErrorMessage = "Please only use Alphabetic Letters, Spaces and Dashes")]
        public string name { get; set; }

        [BsonElement("Email")]
        [Required(ErrorMessage = "Please Enter an Email Address")]
        [AllowNull]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Please Enter a Valid Email Address")]
        public string email { get; set; }

        [BsonElement("Location")]
        [Required]
        [AllowNull]
        public string location { get; set; }

        [BsonElement("Subject")]
        [Required(ErrorMessage = "Please Enter an Subject Line")]
        [AllowNull]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Please Enter a more detailed subject description")]
        [RegularExpression(@"^[^-][A-Za-z0-9\s-]*", ErrorMessage = "Please only use Alpha-Numeric Characters, Spaces and Dashes")]
        public string subject { get; set; }

        [BsonElement("Content")]
        [Required(ErrorMessage = "Please Enter your Message")]
        [AllowNull]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Please enter more detail")]
        [RegularExpression(@"^[^-][A-Za-z0-9\s-].*", ErrorMessage = "Please only use Alpha-Numeric Characters, Spaces and Dashes")]
        public string content { get; set; }

        [BsonElement("Complete")]
        [Required]
        [AllowNull]
        public bool complete { get; set; }
    }
}
