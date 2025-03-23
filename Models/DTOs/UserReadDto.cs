namespace Banko.Models.DTOs
{
  public class UserReadDto
  {
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}
