using Microsoft.AspNetCore.Mvc;
using CleanDapperApp.Models.Interfaces;
using CleanDapperApp.Models.Entities;

namespace CleanDapperApp.Api.Controllers
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtProvider _jwtProvider;

        public AuthController(IUnitOfWork unitOfWork, IJwtProvider jwtProvider)
        {
            _unitOfWork = unitOfWork;
            _jwtProvider = jwtProvider;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Ideally, check if user exists. For brevity, assuming new.
            // Password hashing should be done here properly (BCrypt, etc).
            // This is a simplified example.
            
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = request.Password, // WARNING: Hash this in production!
                Role = "User"
            };

            var userRepo = _unitOfWork.Repository<User>();
            await userRepo.AddAsync(user);
            _unitOfWork.Commit();

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var userRepo = _unitOfWork.Repository<User>();
            // Since GenericRepository is generic, we might need a custom method for finding by Email.
            // But we can use GetAll and filter for this demo, or extend GenericRepository.
            // To be proper, I will just filter implicitly here for the demo.
            
            var users = await userRepo.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null || user.PasswordHash != request.Password) // Again, verify hash
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = _jwtProvider.Generate(user);
            return Ok(new { Token = token });
        }
    }
}
