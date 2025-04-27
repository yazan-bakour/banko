using System.ComponentModel.DataAnnotations;

namespace Banko.Models.DTOs
{
  public class UserRegisterDto
  {
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Password must be at least 5 characters")]
    public string Password { get; set; } = string.Empty;
  }
}
