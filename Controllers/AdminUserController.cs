using Banko.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Banko.Services;

// TODO: Add test.
// TODO: Remove messages from responses.
// TODO: Add get user by id functionality.
// TODO: Add update user functionality.

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
      var users = await userService.GetAllUsersAsync();
      return Ok(users);
    }

    [HttpPut("{id}")]
    // Update user role, for now.
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDto updateDto)
    {
      var user = await userService.GetUserByIdAsync(id);
      if (user == null)
      {
        return NotFound(new { message = "User not found." });
      }

      user.FullName = updateDto.FullName ?? user.FullName;
      user.Role = updateDto.Role;

      await userService.UpdateUserAsync(user);

      return Ok(new { message = "User updated successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
      var user = await userService.GetUserByIdAsync(id);
      if (user == null)
      {
        return NotFound(new { message = "User not found." });
      }

      await userService.DeleteUserAsync(user);

      return Ok(new { message = "User deleted successfully." });
    }
  }
}
