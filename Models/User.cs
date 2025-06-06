using System.ComponentModel.DataAnnotations;
using Banko.Validation;

namespace Banko.Models
{
  public enum UserRole
  {
    Admin,
    Customer,
    Support
  }
  public enum UserGender
  {
    Male,
    Female
  }
  public class User : BaseEntity
  {
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "First user name must be between 2 and 100 characters")]
    public string? FirstName { get; set; } = null;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Last user name must be between 2 and 100 characters")]
    public string? LastName { get; set; } = null;

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

    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Address must not exceed 200 characters")]
    public string? Address { get; set; }

    [StringLength(30, ErrorMessage = "City must not exceed 30 characters")]
    public string? City { get; set; }

    [StringLength(50, ErrorMessage = "State must not exceed 50 characters")]
    public string? State { get; set; }

    [RegularExpression(@"^[a-zA-Z0-9]{4,6}$", ErrorMessage = "ZIP/postal code must be 4-6 characters (letters and/or numbers)")]
    [StringLength(10, ErrorMessage = "ZIP/postal code must not exceed 10 characters")]
    public string? ZipCode { get; set; }

    [StringLength(50, ErrorMessage = "Country must not exceed 50 characters")]
    public string? Country { get; set; }

    [DataType(DataType.Date)]
    [PastDate(ErrorMessage = "Date of birth must be in the past")]
    public DateTime? DateOfBirth { get; set; }

    public DateTime? LastLogin { get; set; }

    [StringLength(20, ErrorMessage = "Nationality must not exceed 50 characters")]
    public string? Nationality { get; set; }

    [EnumDataType(typeof(UserGender), ErrorMessage = "Invalid gender value")]
    public UserGender Gender { get; set; }

    public bool IsVerified { get; set; } = false;
    public string? UniqueId { get; set; } // @username1234

    [Url(ErrorMessage = "Please enter a valid URL for the profile picture")]
    [StringLength(2048, ErrorMessage = "Profile picture URL must not exceed 2048 characters")]
    public string? ProfilePictureUrl { get; set; }

    [StringLength(255, ErrorMessage = "Profile picture file path must not exceed 255 characters")]
    [RegularExpression(@"(?i).*\.(jpg|jpeg|png|gif|bmp|webp)$",
        ErrorMessage = "Profile picture must be a valid image file (jpg, jpeg, png, gif, bmp, or webp)")]
    public string? ProfilePictureFile { get; set; }

    // Helper property (not mapped to database, use [NotMapped] if using EF Core)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? ProfilePictureDisplay => ProfilePictureUrl ?? ProfilePictureFile;
    /// <summary>
    /// User preferences stored as key-value pairs
    /// </summary>
    public Dictionary<string, string>? Preferences { get; set; }
  }
}
