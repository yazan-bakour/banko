using Banko.Models;
using Banko.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Banko.Services;

// Add validation.
// Add logout revokedToken functionality.
// Add forgot password functionality.
// Add reset password functionality.
// Add change password functionality.
// Add test.
// Remove messages from responses.

namespace Banko.Controllers
{
  [Route("api/users")]
  [ApiController]
  // primary constructor syntax
  public class UserController(UserService userService) : ControllerBase
  {
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userDto)
    {
      User? existingUser = await userService.GetUserByEmailAsync(userDto.Email);

      if (existingUser != null)
      {
        return BadRequest(new { message = "Email already registered." });
      }

      string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

      User newUser = new()
      {
        FullName = userDto.FullName,
        Email = userDto.Email,
        PasswordHash = passwordHash
      };

      User? user = await userService.AddUserAsync(newUser);

      if (user == null)
      {
        return BadRequest(new { message = "Failed to register user." });
      }

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
      User? user = await userService.GetUserByEmailAsync(loginDto.Email);
      if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
      {
        return Unauthorized(new { message = "Invalid email or password." });
      }

      string token = userService.GenerateJwtToken(user);

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

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IActionResult), 200)]
    public async Task<IActionResult> GetAdminData()
    {
      IEnumerable<User> admins = await userService.GetUserByRoleAsync(UserRole.Admin);
      return Ok(
        new
        {
          message = "Welcome to the Admin list!",
          admins = admins.Select(admin => new { admin.FullName, admin.Email, admin.CreatedAt })
        });
    }

    [HttpGet("support")]
    [Authorize(Roles = "Support,Admin")]
    [ProducesResponseType(typeof(IActionResult), 200)]
    public async Task<IActionResult> GetCustomerSupportData()
    {
      IEnumerable<User> supports = await userService.GetUserByRoleAsync(UserRole.Support);
      return Ok(
        new
        {
          message = "Welcome to the Customers support list!",
          supports = supports.Select(support => new { support.FullName, support.Email, support.CreatedAt })
        });
    }

    [HttpGet("customer")]
    [Authorize(Roles = "Customer,Support,Admin")]
    [ProducesResponseType(typeof(IActionResult), 200)]
    public async Task<IActionResult> GetCustomerData()
    {
      IEnumerable<User> customers = await userService.GetUserByRoleAsync(UserRole.Customer);
      return Ok(
        new
        {
          message = "Welcome to the Customers list!",
          customers = customers.Select(customer => new { customer.FullName, customer.Email, customer.CreatedAt })
        });
    }
  }
}
