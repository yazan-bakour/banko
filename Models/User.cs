using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Banko.Models
{
  public enum UserRole
  {
    Admin,
    Customer,
    Support
  }
  public class User
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Password must be between 5 and 100 characters")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public UserRole Role { get; set; } = UserRole.Customer;
  }
}
