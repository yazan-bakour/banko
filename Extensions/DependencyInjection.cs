using Banko.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Banko.Data;

namespace Banko.Extensions
{
  public static class DependencyInjection
  {
    private static readonly string[] EmptyArray = Array.Empty<string>();

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
      services.AddScoped<AccountService>();
      services.AddScoped<UserService>();
      services.AddScoped<FundsService>();

      services.AddControllers();
      services.AddEndpointsApiExplorer();

      return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
      services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

      return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
      var jwtSettings = configuration.GetSection("Jwt");
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT key is missing.")));

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = key
          };
        });

      return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
      services.AddSwaggerGen(options =>
      {
        options.SwaggerDoc("v1", new() { Title = "Banko API", Version = "v1" });

        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
          Name = "Authorization",
          Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
          Scheme = "Bearer",
          BearerFormat = "JWT",
          In = Microsoft.OpenApi.Models.ParameterLocation.Header,
          Description = "Enter your valid token in the text input below.\r\n\r\nExample: \"eyJhbGciOiJIUzI1NiIsInR...\""
        });

        options.AddSecurityRequirement(new()
        {
          {
            new()
            {
              Reference = new()
              {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
              }
            },
            EmptyArray
          }
        });
      });

      return services;
    }
  }
}
