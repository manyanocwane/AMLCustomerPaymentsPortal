using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using AMLCustomerPaymentsPortal.Server.Models;    
using AMLCustomerPaymentsPortal.Utilities;        
using AMLCustomerPaymentsPortal.Server.Data;     
using Microsoft.EntityFrameworkCore;              
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;         // To read JWT settings from appsettings.json

namespace AMLCustomerPaymentsPortal.Server.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        [AllowAnonymous] // Allow anyone to access registration
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user already exists by Email, IdNumber, or AccountNumber
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return Conflict(new { message = "User with this email already exists." });
            }
            if (await _context.Users.AnyAsync(u => u.IdNumber == model.IdNumber))
            {
                return Conflict(new { message = "User with this ID number already exists." });
            }
            if (await _context.Users.AnyAsync(u => u.AccountNumber == model.AccountNumber))
            {
                return Conflict(new { message = "User with this account number already exists." });
            }

            var user = new User
            {
                FullName = model.FullName,
                IdNumber = model.IdNumber,
                AccountNumber = model.AccountNumber,
                Email = model.Email,
                PasswordHash = PasswordHelperUtility.HashPassword(model.Password), 
                IsEmployee = false, // Default new registrations to non-employee
                RegistrationDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        [AllowAnonymous] // Allow anyone to access login
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !PasswordHelperUtility.VerifyPassword(model.Password, user.PasswordHash))
            {
                // Generic message for security
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { message = "Login successful.", token = token, userId = user.Id, email = user.Email, isEmployee = user.IsEmployee });
        }

       
        [HttpGet("profile")]
        [Authorize] // Requires a valid JWT token
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User ID claim not found or invalid in token." });
            }

            var user = await _context.Users
                                   .Select(u => new { u.Id, u.FullName, u.Email, u.AccountNumber, u.IsEmployee })
                                   .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { message = "User profile not found." });
            }

            return Ok(user);
        }


        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Use DB User Id as primary identifier in token
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("fullName", user.FullName), 
                new Claim("accountNumber", user.AccountNumber),
                new Claim(ClaimTypes.Role, user.IsEmployee ? "Employee" : "Customer") 
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiry (e.g., 1 hour)
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}