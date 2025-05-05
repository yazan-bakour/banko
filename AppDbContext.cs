using Microsoft.EntityFrameworkCore;
using Banko.Models;

namespace Banko.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Account> Accounts { get; set; }
		public DbSet<RevokedToken> RevokedTokens { get; set; }
		public DbSet<Funds> Funds { get; set; }
		public DbSet<Transactions> Transactions { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.UseHiLo();
		}
	}
}
