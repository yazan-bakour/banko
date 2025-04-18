using Serilog;
using Serilog.Events;

namespace Banko.Services
{
  public static class LoggingServiceExtensions
  {
    public static IServiceCollection AddFileLogging(this IServiceCollection services, string logFilePath = "Logs/banko-.log")
    {
      Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Debug()
          // Override some database information to Filter out EF Core logs
          .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Model", LogEventLevel.Warning)
          // Other Microsoft namespaces to filter as needed
          .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
          .Enrich.FromLogContext()
          .WriteTo.Console()
          .WriteTo.File(
              path: logFilePath,
              rollingInterval: RollingInterval.Day,
              fileSizeLimitBytes: 10 * 1024 * 1024,
              retainedFileCountLimit: 30,
              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
          )
          .CreateLogger();

      services.AddLogging(loggingBuilder =>
      {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog(dispose: true);
      });

      return services;
    }
  }
}