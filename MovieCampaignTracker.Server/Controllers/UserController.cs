using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MovieCampaignTracker.Server.Data;
using MovieCampaignTracker.Server.Models;
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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Email", user.Email);
            parameters.Add("@PasswordHash", user.PasswordHash);
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User loginUser)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Email", loginUser.Email);
            parameters.Add("@PasswordHash", loginUser.PasswordHash);

            var user = (await _dbHelper.QueryStoredProcedureAsync<User>("LoginUser", parameters)).FirstOrDefault();
            if (user == null) return Unauthorized("Invalid credentials.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { Token = tokenHandler.WriteToken(token), Role = user.Role ?? "User" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }
}
