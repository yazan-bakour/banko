using Banko.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Banko.Services;
using Banko.Models;

// Add test.

namespace Banko.Controllers
{
  [ApiController]
  [Route("api/admin/users")]
  [Authorize(Roles = "Admin")]
  public class AdminUsersController(UserService userService) : ControllerBase
  {
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
    {
      IEnumerable<User> users = await userService.GetAllUsersAsync();
      return Ok(users);
    }

    [HttpPut("{id}")]
    // Update user role, for now.
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDto updateDto)
    {
      User? user = await userService.GetUserByIdAsync(id);
      if (user == null)
      {
        return NotFound(new { message = "User not found." });
      }

      user.FullName = updateDto.FullName ?? user.FullName;
      user.Role = updateDto.Role;
      user.UpdatedAt = DateTime.UtcNow;

      await userService.UpdateUserAsync(user);

      return Ok(new { message = "User updated successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
      User? user = await userService.GetUserByIdAsync(id);
      if (user == null)
      {
        return NotFound(new { message = "User not found." });
      }

      await userService.DeleteUserAsync(user);

      return Ok(new { message = "User deleted successfully." });
    }
  }
}
