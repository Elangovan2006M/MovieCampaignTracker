using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MovieCampaignTracker.Infrastructure.Data;
using MovieCampaignTracker.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DatabaseHelper _dbHelper;
    private readonly IConfiguration _configuration;

    public UserController(DatabaseHelper dbHelper, IConfiguration configuration)
    {
        _dbHelper = dbHelper;
        _configuration = configuration;
    }

    // ✅ Register API (Stores Hashed Password)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        try
        {
            // Hash the password before storing
            var passwordHasher = new PasswordHasher<User>();
            string hashedPassword = passwordHasher.HashPassword(user, user.PasswordHash);
            user.PasswordHash = hashedPassword;

            var parameters = new DynamicParameters();
            parameters.Add("@Email", user.Email);
            parameters.Add("@PasswordHash", user.PasswordHash); // Stores hashed password
            parameters.Add("@Role", user.Role);

            int result = await _dbHelper.ExecuteStoredProcedureAsync("RegisterUser", parameters);

            return result switch
            {
                1 => Ok("User registered successfully."),
                -1 => BadRequest("Registration failed: Email already exists."),
                _ => BadRequest($"Registration failed: Unexpected result {result}")
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    // ✅ Login API (Verifies Hashed Password)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User loginUser)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Email", loginUser.Email);

            var storedUser = (await _dbHelper.QueryStoredProcedureAsync<User>("GetUserByEmail", parameters)).FirstOrDefault();
            if (storedUser == null)
                return Unauthorized("Invalid credentials.");

            // Verify hashed password
            var passwordHasher = new PasswordHasher<User>();
            var verificationResult = passwordHasher.VerifyHashedPassword(storedUser, storedUser.PasswordHash, loginUser.PasswordHash);

            if (verificationResult != PasswordVerificationResult.Success)
                return Unauthorized("Invalid credentials.");

            // Generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, storedUser.Email),
                    new Claim(ClaimTypes.Role, storedUser.Role ?? "User")
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { Token = tokenHandler.WriteToken(token), Role = storedUser.Role ?? "User" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }
}