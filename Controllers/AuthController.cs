using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using pr8_trpo.Models;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace pr8_trpo.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly TasksDbContext _context;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _configuration;
        public AuthController(TasksDbContext context, IConfiguration configuration, IPasswordHasher<User> hasher)
        {
            _context = context;
            _configuration = configuration;
            _hasher = hasher;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "TasksApi",
                audience: "TasksApiClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public record AuthRequest(string Login, string Password);

        [HttpPost("register")]
        public IActionResult Register(AuthRequest request)
        {
            if (_context.Users.Any(u => u.Login == request.Login))
                return Conflict("Логин уже существует");

            var user = new User
            {
                Login = request.Login,
                Role = "User"
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("Регистрация успешна");
        }

        [HttpPost("login")]
        public IActionResult Login(AuthRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == request.Login);

            if (user == null)
                return Unauthorized("Неверный логин/пароль");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Неверный логин/пароль");

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                access_token = token
            });
        }
    }
}
