using Banko.Data;
using Banko.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Banko.Controllers
{
  [ApiController]
  [Route("api/admin/users")]
  [Authorize(Roles = "Admin")]
  public class AdminUsersController : ControllerBase
  {
    private readonly AppDbContext _context;

    public AdminUsersController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
    {
      var users = await _context.Users
          .Select(u => new UserReadDto
          {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt
          })
          .ToListAsync();

      return Ok(users);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDto updateDto)
    {
      var user = await _context.Users.FindAsync(id);
      if (user == null)
      {
        return NotFound(new { message = "User not found." });
      }

      user.FullName = updateDto.FullName ?? user.FullName;
      user.Role = updateDto.Role;

      _context.Users.Update(user);
      await _context.SaveChangesAsync();

      return Ok(new { message = "User updated successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
      var user = await _context.Users.FindAsync(id);
      if (user == null)
      {
        return NotFound(new { message = "User not found." });
      }

      _context.Users.Remove(user);
      await _context.SaveChangesAsync();

      return Ok(new { message = "User deleted successfully." });
    }
  }
}
