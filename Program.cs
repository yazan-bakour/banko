using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Banko.Extensions;
using Banko.Filters;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
	options.AddPolicy("CorsPolicy",
		policyBuilder =>
		{
			policyBuilder
				.WithOrigins(allowedOrigins)
				.AllowCredentials()
				.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
				.WithHeaders("Authorization", "Content-Type");
		});
});

builder.Services.AddSwagger();
builder.Services.AddSwaggerGen(c =>
{
	c.SchemaFilter<SwaggerEnumSchemaFilter>();
	c.EnableAnnotations();

	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	c.IncludeXmlComments(xmlPath);

	c.CustomSchemaIds(type => type.Name);
});

builder.Services.AddApplicationServices();

var databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
var databasePassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

if (string.IsNullOrEmpty(jwtSecret))
{
	throw new InvalidOperationException("JWT_SECRET_KEY is missing in the .env file!");
}

builder.Configuration["Jwt:Key"] = jwtSecret;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
	throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

connectionString = connectionString
		.Replace("PLACEHOLDER_DB_NAME", databaseName)
		.Replace("PLACEHOLDER_DB_PASSWORD", databasePassword);

builder.Services.AddDatabase(connectionString);
builder.Services.AddJwtAuthentication(builder.Configuration);

// builder.Services.AddHsts(options =>
// {
//     options.Preload = true;
//     options.IncludeSubDomains = true;
//     options.MaxAge = TimeSpan.FromDays(60);
//     options.ExcludedHosts.Add("example.com");
//     options.ExcludedHosts.Add("www.example.com");
// });

builder.Services.AddHttpsRedirection(options =>
{
	options.HttpsPort = 7296;
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
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();