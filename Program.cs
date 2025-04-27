using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Banko.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddApplicationServices();
builder.Services.AddSwagger();

var databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
var databasePassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var clientUrl = Environment.GetEnvironmentVariable("ClIENT_HTTP_URL");

if (string.IsNullOrEmpty(jwtSecret))
{
	throw new InvalidOperationException("JWT_SECRET_KEY is missing in the .env file!");
}
if (string.IsNullOrEmpty(clientUrl))
{
	throw new InvalidOperationException("ClIENT_HTTP_URLis missing in the .env file!");
}

builder.Configuration["Jwt:Key"] = jwtSecret;
builder.Configuration["CLIENT_URL:ClIENT_HTTP_URL"] = clientUrl;

if (string.IsNullOrEmpty(connectionString))
{
	throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

connectionString = connectionString
		.Replace("PLACEHOLDER_DB_NAME", databaseName)
		.Replace("PLACEHOLDER_DB_PASSWORD", databasePassword);

builder.Services.AddDatabase(connectionString);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowBlazorClient", policy =>
	{
		policy.WithOrigins(clientUrl)
			.AllowAnyMethod()
			.AllowAnyHeader();
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
app.UseCors("AllowBlazorClient");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();