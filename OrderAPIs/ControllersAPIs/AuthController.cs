using Microsoft.AspNetCore.Mvc;
using OrderAPIs.Extensions;
using OrderAPIs.Models;

namespace OrderAPIs.ControllersAPIs;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Email == "admin@logipulse.com" && request.Password == "Admin@123")
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Role = "Admin"
            };

            var token = user.GenerateJwtToken(_configuration);

            return Ok(new { token, user = new { user.Email, user.Role } });
        }

        return Unauthorized(new { message = "Invalid credentials" });
    }
}

public record LoginRequest(string Email, string Password);
