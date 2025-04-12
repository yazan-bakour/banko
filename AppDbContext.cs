using Microsoft.EntityFrameworkCore;
using Banko.Models;

namespace Banko.Data
{
	public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
	{
		public DbSet<User> Users { get; set; }
		public DbSet<Account> Accounts { get; set; }
		public DbSet<RevokedToken> RevokedTokens { get; set; }
		public DbSet<Funds> Funds { get; set; }
		public DbSet<Transactions> Transactions { get; set; }
	}

}