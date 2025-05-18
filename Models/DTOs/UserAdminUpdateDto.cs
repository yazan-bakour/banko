namespace Banko.Models.DTOs
{
  public class UserAdminUpdateDto
  {
    public string? FullName { get; set; }
    public UserRole Role { get; set; }
    // public bool IsVerified { get; set; } = false;
  }
}
