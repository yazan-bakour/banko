using Banko.Data;
using Banko.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// TODO: move GenerateJwtToken to Utils.
// TODO: Use explicit type declaration instead of var

namespace Banko.Services
{
  public class UserService(AppDbContext context, IConfiguration configuration)
  {
    private readonly AppDbContext _context = context;
    private readonly IConfiguration _configuration = configuration;

    public string GenerateJwtToken(User user)
    {
      var jwtSettings = _configuration.GetSection("Jwt");
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT key is missing")));

      var claims = new[]
      {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString())
      };

      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: jwtSettings["Issuer"],
          audience: jwtSettings["Audience"],
          claims: claims,
          expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"] ?? throw new InvalidOperationException("JWT expires in minutes is missing"))),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
      return await _context.Users.ToListAsync();
    }

    public async Task<User?> AddUserAsync(User user)
    {
      await _context.Users.AddAsync(user);
      await _context.SaveChangesAsync();
      return user;
    }

    public async Task<User?> UpdateUserAsync(User user)
    {
      _context.Users.Update(user);
      await _context.SaveChangesAsync();
      return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
      return await _context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetUserByRoleAsync(UserRole role)
    {
      return await _context.Users.Where(u => u.Role == role).ToListAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
      return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> DeleteUserAsync(User user)
    {
      _context.Users.Remove(user);
      await _context.SaveChangesAsync();
      return user;
    }
  }
}