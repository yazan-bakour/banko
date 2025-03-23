namespace Banko.Models
{
	public class RevokedToken
	{
		public int Id { get; set; }
		public required string Token { get; set; }
		public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
	}
}
