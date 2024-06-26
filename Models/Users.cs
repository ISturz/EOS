using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace EOS.Models
{
    [BsonIgnoreExtraElements]
    public class Users
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Email")]
        [Required(ErrorMessage = "Please Enter an Email Address")]
        [StringLength(50)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&*+/=?^_`{|}~-]+@(?!justadumbdomain)[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Please Enter a Valid Email Address")]
        public string Email { get; set; } = null!;

        [BsonElement("Password")]
        [Required(ErrorMessage = "Please Enter a Password")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[-!$%^&*()_+|~=`{}\[\];<>?,.\/])^((?!:)(?!').)+$", ErrorMessage = "Passwords must contain an Upper, a Lower and a Special Character. Please don't use ' or spaces")]
        [StringLength(25, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 25 characters")]

        public string Password { get; set; } = null!;

        // Does not require validation as this is fed by the application
        [BsonElement("Generated Emails")]
        [AllowNull]
        [StringLength(50)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Please Enter a Valid Email Address")]
        public List<KeyValuePair<string, string>> GeneratedEmails { get; set; }


        [BsonElement("DeletedEmails")]
        [AllowNull]
        [StringLength(50)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Please Enter a Valid Email Address")]
        public List<KeyValuePair<string, string>> DeletedEmails { get; set; }



        
        [BsonElement("RememberMe")]
        public bool RememberMe { get; set; }

    }

    public class NewUser : Users
    {
        [BsonElement("firstName")]
        [Required(ErrorMessage = "Please Enter a First Name")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 15 characters")]
        [RegularExpression(@"^[^-][A-Za-z\s-]*", ErrorMessage = "Please only use Alphabetic Letters, Spaces and Dashes")]
        public string firstname { get; set; }

        [Required(ErrorMessage = "Please Enter a Last Name")]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 15 characters")]
        [RegularExpression(@"^[^-][A-Za-z\s-]*", ErrorMessage = "Please only use Alphabetic Letters and Spaces")]
        [BsonElement("lastName")]
        public  string lastName { get; set; }

        [Required(ErrorMessage = "Please Enter a Country")]
        [StringLength(50)]
        [RegularExpression(@"^[^-][A-Za-z\s-]*", ErrorMessage = "Please only use Alphabetic Letters and Spaces")]
        [BsonElement("location")]
        public  string location { get; set; }

        [Required(ErrorMessage = "Please Re-Enter Password")]
        [StringLength(25)]
        public string ConfirmPassword { get; set; }

        

        
    }

    public class LoggedIn : NewUser
    {
        [BsonElement("Account")]
        public string Account { get; set; }


    }

    public class NewGeneratedEmail
    {
        [BsonElement("Account")]
        public string Account { get; set; }
    }

    public class EditEmailModel
    {
        public string Website { get; set; }

        public string GenEmail { get; set; }
    }


}
