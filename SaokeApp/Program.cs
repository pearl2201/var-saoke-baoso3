using Npgsql;
using SaokeApp;
using SaokeApp.Data;
using Serilog;

using SerilogTracing;
using System.Net.WebSockets;

/// <summary>
/// Starter
/// </summary>
public class Program
{
    /// <summary>
    /// Main function
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task Main(string[] args)
    {
        using var listener = new ActivityListenerConfiguration().Instrument.AspNetCoreRequests().TraceToSharedLogger();
        NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
        NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
        var host = CreateHostBuilder(args).Build();
        using (var scope = host.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
            await dbInitializer.InitialiseAsync();
            await dbInitializer.SeedAsync();
        }
        await host.RunAsync();
    }

    /// <summary>
    /// WebHost builder
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
              .ConfigureAppConfiguration((hostingContext, config) =>
              {
                  config.AddJsonFile(
                      "appsettings.Local.json",
                       optional: true,
                       reloadOnChange: false);
              })
            .UseSerilog(((context, configuration) =>
            {
                configuration.Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("Application", "Herobase.Api")
                    .ReadFrom.Configuration(context.Configuration);
            }))
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
