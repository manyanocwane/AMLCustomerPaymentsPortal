using System.ComponentModel.DataAnnotations;

namespace AMLCustomerPaymentsPortal.Server.Models
{
    public class User
    {
        [Key] // Primary Key for the database
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot be longer than 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Full name contains invalid characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "ID number is required.")]
        [StringLength(50)] // Adjust max length as needed
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "ID number must be alphanumeric.")] 
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "Account number is required.")]
        [StringLength(50)] 
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Account number must be alphanumeric.")] 
        public string AccountNumber { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } // This will be used as the username

        [Required]
        public string PasswordHash { get; set; } // Stores the hashed password

        public bool IsEmployee { get; set; } = false; // Default to customer

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    }
}