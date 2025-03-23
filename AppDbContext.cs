using Microsoft.EntityFrameworkCore;
using Banko.Models;

namespace Banko.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Account> Accounts { get; set; }
		public DbSet<RevokedToken> RevokedTokens { get; set; }
	}

}