using Banko.Data;
using Banko.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Banko.Models.DTOs;
using Banko.Helpers;

// move GenerateJwtToken to AuthService.

namespace Banko.Services
{
  public class UserService(AppDbContext context, IConfiguration configuration, UserHelper userHelper)
  {
    private readonly AppDbContext _context = context;
    private readonly IConfiguration _configuration = configuration;

    public string GenerateJwtToken(User user)
    {
      IConfiguration jwtSettings = _configuration.GetSection("Jwt");
      SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT key is missing")));

      Claim[] claims =
      [
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new(ClaimTypes.Role, user.Role.ToString())
      ];

      SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

      JwtSecurityToken token = new(
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

    public async Task<ServiceResult<User>> UpdateUserSettingsAsync(int userId, UserSettingsDto settings)
    {

      User? user = await _context.Users.FindAsync(userId);

      if (user == null)
      {
        return ServiceResult<User>.Failure("User not found");
      }

      if (!string.IsNullOrWhiteSpace(settings.Email) && settings.Email != user.Email)
      {
        bool inUse = await _context.Users
            .AnyAsync(u => u.Email == settings.Email && u.Id != user.Id);
        if (inUse)
          return ServiceResult<User>.Failure("Email is already in use");
        user.Email = settings.Email;
      }

      if (!string.IsNullOrEmpty(settings.NewPassword))
      {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(settings.NewPassword);
      }

      if (settings.FirstName != null || settings.LastName != null)
      {
        user.FirstName = settings.FirstName;
        user.LastName = settings.LastName;
        user.FullName = $"{user.FirstName} {user.LastName}";

        string baseId = UserHelper.GenerateUserIdBase(user.FirstName ?? string.Empty, user.LastName ?? string.Empty);
        if (!await FindUniqueUserIdAsync(baseId))
          user.UniqueId = baseId;
        else
          user.UniqueId = UserHelper.GenerateUserIdVariant(baseId);
      }

      if (settings!.Gender.HasValue)
      {
        user.Gender = settings.Gender.Value;
      }

      var dobResult = UserHelper.ProcessAndValidateDateOfBirth(settings.DateOfBirth);
      if (!dobResult.IsSuccess)
      {
        return ServiceResult<User>.Failure(dobResult.ErrorMessage ?? "Invalid date of birth.");
      }
      if (dobResult.Data.HasValue)
      {
        user.DateOfBirth = dobResult.Data.Value;
      }

      if (settings.PhoneNumber != null) user.PhoneNumber = settings.PhoneNumber;
      if (settings.Address != null) user.Address = settings.Address;
      if (settings.City != null) user.City = settings.City;
      if (settings.State != null) user.State = settings.State;
      if (settings.ZipCode != null) user.ZipCode = settings.ZipCode;
      if (settings.Country != null) user.Country = settings.Country;
      if (settings.Nationality != null) user.Nationality = settings.Nationality;
      if (settings.Preferences != null) user.Preferences = settings.Preferences;
      if (settings.ProfilePictureUrl != null) user.ProfilePictureUrl = settings.ProfilePictureUrl;
      if (settings.ProfilePictureDisplay != null) user.ProfilePictureUrl = settings.ProfilePictureDisplay;
      // if (settings.ProfilePictureFile != null) user.ProfilePictureFile = settings.ProfilePictureFile;
      user.UpdatedAt = DateTime.UtcNow;

      try
      {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return ServiceResult<User>.Success(user);
      }
      catch (DbUpdateException ex)
      {
        var errorMessage = "Failed to update user settings due to a database error.";
        if (ex.InnerException != null)
        {
          errorMessage += $" Details: {ex.InnerException.Message}";
        }
        return ServiceResult<User>.Failure(errorMessage);
      }
      catch (Exception ex) // Catch other general exceptions
      {
        return ServiceResult<User>.Failure($"Failed to update user settings: {ex.Message}");
      }
    }
    private async Task<bool> FindUniqueUserIdAsync(string uniqueId)
    {
      bool exists = await _context.Users.AnyAsync(u => u.UniqueId == uniqueId);

      return exists;
    }

    public static async Task<string?> UploadProfilePictureAsync(IFormFile file)
    {
      if (file == null || file.Length == 0)
        return null;

      var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
      if (!allowedTypes.Contains(file.ContentType.ToLower()))
        throw new InvalidOperationException("Invalid file type. Only JPEG, PNG, and GIF are allowed.");

      string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
      string savePath = Path.Combine("wwwroot", "uploads", fileName);

      Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

      using (var fileStream = new FileStream(savePath, FileMode.Create))
      {
        await file.CopyToAsync(fileStream);
      }

      return $"uploads/{fileName}";
    }
  }
}