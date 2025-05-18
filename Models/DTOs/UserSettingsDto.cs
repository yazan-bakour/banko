using System.ComponentModel.DataAnnotations;
using Banko.Validation;

namespace Banko.Models.DTOs;

public class UserSettingsDto
{
  [Required(ErrorMessage = "First name is required")]
  [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
  public string FirstName { get; set; } = string.Empty;

  [Required(ErrorMessage = "Last name is required")]
  [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters")]
  public string LastName { get; set; } = string.Empty;

  [EmailAddress(ErrorMessage = "Invalid email format")]
  [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
  public string? Email { get; set; } = string.Empty;

  [StringLength(100, MinimumLength = 5, ErrorMessage = "Password must be between 5 and 100 characters")]
  public string? NewPassword { get; set; } = string.Empty;

  [Phone(ErrorMessage = "Please enter a valid phone number")]
  [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
  public string? PhoneNumber { get; set; }

  [StringLength(100, ErrorMessage = "Address must not exceed 200 characters")]
  public string? Address { get; set; }

  [StringLength(30, ErrorMessage = "City must not exceed 30 characters")]
  public string? City { get; set; }

  [StringLength(50, ErrorMessage = "State must not exceed 50 characters")]
  public string? State { get; set; }

  [RegularExpression(@"^[a-zA-Z0-9]{4,8}$", ErrorMessage = "ZIP/postal code must be 4-8 characters (letters and/or numbers)")]
  [StringLength(10, ErrorMessage = "ZIP/postal code must not exceed 10 characters")]
  public string? ZipCode { get; set; }

  [StringLength(50, ErrorMessage = "Country must not exceed 50 characters")]
  public string? Country { get; set; }

  [DataType(DataType.Date)]
  [PastDate(ErrorMessage = "Date of birth must be in the past")]
  public DateTime? DateOfBirth { get; set; }

  [StringLength(20, ErrorMessage = "Nationality must not exceed 50 characters")]
  public string? Nationality { get; set; }

  [EnumDataType(typeof(UserGender), ErrorMessage = "Invalid gender value")]
  public UserGender? Gender { get; set; }

  [Url(ErrorMessage = "Please enter a valid URL for the profile picture")]
  [StringLength(2048, ErrorMessage = "Profile picture URL must not exceed 2048 characters")]
  public string? ProfilePictureUrl { get; set; }

  [StringLength(255, ErrorMessage = "Profile picture file path must not exceed 255 characters")]
  [RegularExpression(@"(?i).*\.(jpg|jpeg|png|gif|bmp|webp)$",
      ErrorMessage = "Profile picture must be a valid image file (jpg, jpeg, png, gif, bmp, or webp)")]
  public string? ProfilePictureFile { get; set; }
}