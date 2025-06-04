//using System.ComponentModel.DataAnnotations;
//namespace AMLCustomerPaymentsPortal.Models
//{
//    public class RegisterRequestModel
//    {
//        [Required]
//        [EmailAddress]
//        public string Email { get; set; }

//        [Required]
//        [StringLength(100, MinimumLength = 8)]
//        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$",
//            ErrorMessage = "Password must be at least 8 characters long and include letters and numbers.")]
//        public string Password { get; set; }
//    }

//    public class LoginRequest
//    {
//        [Required]
//        [EmailAddress]
//        public string Email { get; set; }

//        [Required]
//        public string Password { get; set; }
//    }
//}


using System.ComponentModel.DataAnnotations;

namespace AMLCustomerPaymentsPortal.Server.Models // Ensure this namespace matches your project
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Full name contains invalid characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "ID number is required.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "ID number is too short or too long.")] // Adjust lengths
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "ID number must be alphanumeric.")]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "Account number is required.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Account number is too short or too long.")] // Adjust lengths
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Account number must be alphanumeric.")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(256)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_#-])[A-Za-z\d@$!%*?&_#-]{8,}$",
            ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}