using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using Banko.Data;
using Banko.Services;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<UserService>();

var databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
var databasePassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

if (string.IsNullOrEmpty(jwtSecret))
{
	throw new InvalidOperationException("JWT_SECRET_KEY is missing in the .env file!");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
	throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

connectionString = connectionString
		.Replace("PLACEHOLDER_DB_NAME", databaseName)
		.Replace("PLACEHOLDER_DB_PASSWORD", databasePassword);

builder.Services.AddDbContext<AppDbContext>(options =>
		options.UseNpgsql(connectionString));

builder.Configuration["Jwt:Key"] = jwtSecret;

var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT key is missing.")))
			};

			// options.Events = new JwtBearerEvents
			// {
			// 	OnTokenValidated = async context =>
			// 	{
			// 		var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
			// 		var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

			// 		var isRevoked = await dbContext.RevokedTokens.AnyAsync(rt => rt.Token == token);
			// 		if (isRevoked)
			// 		{
			// 			context.Fail("This token has been revoked.");
			// 		}
			// 	}
			// };
		});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
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

	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
		{
				{
						new Microsoft.OpenApi.Models.OpenApiSecurityScheme
						{
								Reference = new Microsoft.OpenApi.Models.OpenApiReference
								{
										Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
										Id = "Bearer"
								}
						},
						new string[] {}
				}
		});
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();