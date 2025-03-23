namespace Banko.Models.DTOs
{
  public class UserUpdateDto
  {
    public string? FullName { get; set; }
    public UserRole Role { get; set; }
  }
}
