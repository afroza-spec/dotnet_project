using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MyWebAPI.Models;


[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (model.Password != model.ConfirmPassword)
            return BadRequest("Passwords do not match.");

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            Role = model.Role
        };

        var password = model.Password ?? throw new ArgumentNullException("Password is required.");
        var result = await _userManager.CreateAsync(user, password);


        if (result.Succeeded)
        {
            var role = model.Role ?? throw new ArgumentNullException("Role is required.");
            await _userManager.AddToRoleAsync(user, role);

            return Ok(new { message = "User registered successfully" });
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var email = model.Email ?? throw new ArgumentNullException("Email is required.");
        var user = await _userManager.FindByEmailAsync(email);

        var password = model.Password ?? throw new ArgumentNullException("Password is required.");
        if (user != null && await _userManager.CheckPasswordAsync(user, password))

        {
            var userRole = await _userManager.GetRolesAsync(user);
            var role = userRole.FirstOrDefault() ?? throw new ArgumentNullException("User role is missing.");
            var token = GenerateJwtToken(user, role);


            return Ok(new { token, role = userRole.FirstOrDefault() });
        }
        return Unauthorized("Invalid credentials");
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var jwtKey = _configuration["JwtSettings:Key"]
             ?? throw new ArgumentNullException("JWT Key is missing in appsettings.json");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName ?? throw new ArgumentNullException("UserName is missing.")),

            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            _configuration["JwtSettings:Issuer"],
            _configuration["JwtSettings:Audience"],
            claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
