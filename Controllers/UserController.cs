using Banko.Data;
using Banko.Models;
using Banko.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Banko.Controllers
{
  [Route("api/users")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public UserController(AppDbContext context, IConfiguration configuration)
    {
      _context = context;
      _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userDto)
    {
      var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);

      if (existingUser != null)
      {
        return BadRequest(new { message = "Email already registered." });
      }

      var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

      var newUser = new User
      {
        FullName = userDto.FullName,
        Email = userDto.Email,
        PasswordHash = passwordHash
      };

      _context.Users.Add(newUser);
      await _context.SaveChangesAsync();

      // Return basic info (never return password hashes!)
      return Ok(new
      {
        message = "Registration successful.",
        user = new
        {
          newUser.Id,
          newUser.FullName,
          newUser.Email
        }
      });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto loginDto)
    {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
      if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
      {
        return Unauthorized(new { message = "Invalid email or password." });
      }

      var token = GenerateJwtToken(user);

      return Ok(new
      {
        token,
        user = new
        {
          user.Id,
          user.FullName,
          user.Email,
          user.Role
        }
      });
    }

    // [HttpPost("logout")]
    // [Authorize]
    // public async Task<IActionResult> Logout()
    // {
    //   var tokenParts = Request.Headers.Authorization.ToString().Split(" ");
    //   var token = tokenParts[tokenParts.Length - 1];
    //   if (string.IsNullOrEmpty(token))
    //     return BadRequest(new { message = "No token provided." });

    //   var revokedToken = new RevokedToken
    //   {
    //     Token = token
    //   };

    //   _context.RevokedTokens.Add(revokedToken);
    //   await _context.SaveChangesAsync();

    //   return Ok(new { message = "Logged out and token revoked successfully." });
    // }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IActionResult), 200)]
    public IActionResult GetAdminData()
    {
      var admins = _context.Users.Where(u => u.Role == UserRole.Admin).ToList();
      return Ok(new { message = "Welcome to the Admin list!", admins = admins.Select(admin => new { admin.FullName, admin.Email, admin.CreatedAt }) });
    }

    [HttpGet("support")]
    [Authorize(Roles = "Support,Admin")]
    [ProducesResponseType(typeof(IActionResult), 200)]
    public IActionResult GetCustomerSupportData()
    {
      var supports = _context.Users.Where(u => u.Role == UserRole.Support).ToList();
      return Ok(new { message = "Welcome to the Customers support list!", supports = supports.Select(support => new { support.FullName, support.Email, support.CreatedAt }) });
    }

    [HttpGet("customer")]
    [Authorize(Roles = "Customer,Support,Admin")]
    [ProducesResponseType(typeof(IActionResult), 200)]
    public IActionResult GetCustomerData()
    {
      var customers = _context.Users.Where(u => u.Role == UserRole.Customer).ToList();
      return Ok(new { message = "Welcome to the Customers list!", customers = customers.Select(customer => new { customer.FullName, customer.Email, customer.CreatedAt }) });
    }

    private string GenerateJwtToken(User user)
    {
      var jwtSettings = _configuration.GetSection("Jwt");
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

      var claims = new[]
      {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString())
      };

      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: jwtSettings["Issuer"],
          audience: jwtSettings["Audience"],
          claims: claims,
          expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"])),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
