using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ms_agent_framework;

public static class Program
{
    public static async Task Main(string[] args)
    {
        // Load environment variables from a .env file (if present)
        EnvLoader.Load();

        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => {
                    config.ClearProviders();
                    config.AddConsole();
                    config.SetMinimumLevel(LogLevel.Information);
                });

                // Discover all implementations of ISubApplication in this assembly and register them
                var asm = Assembly.GetExecutingAssembly();
                var appTypes = asm.GetTypes()
                    .Where(t => typeof(ISubApplication).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var t in appTypes)
                {
                    services.AddTransient(typeof(ISubApplication), t);
                }
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
        try
        {
            await host.StartAsync();

            var apps = host.Services.GetServices<ISubApplication>().ToList();

            if (!apps.Any())
            {
                logger.LogWarning("No sub-applications were discovered.");
            }

            Console.WriteLine("Available applications:");
            for (int i = 0; i < apps.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {apps[i].AppName} - {apps[i].AppDescription}");
            }

            Console.WriteLine("[0] Run all");
            Console.Write("Select an app to run (0 for all): ");
            var input = Console.ReadLine();

            if (input == "0")
            {
                foreach (var app in apps)
                {
                    logger.LogInformation("Running {AppName}", app.AppName);
                    await app.RunAsync();
                }
            }
            else if (int.TryParse(input, out var idx) && idx >= 1 && idx <= apps.Count)
            {
                var app = apps[idx - 1];
                logger.LogInformation("Running {AppName}", app.AppName);
                await app.RunAsync();
            }
            else
            {
                logger.LogInformation("No valid selection provided; exiting.");
            }

            await host.StopAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception running agent");
            throw;
        }
    }
}