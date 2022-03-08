using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogRedirect.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace LogRedirect
{
    internal sealed class Program
    {
        private static async Task Main(string[]? args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting Log Redirect");
                await CreateHostBuilder(args).RunConsoleAsync();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Log redirect terminated");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[]? args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(s => s.AddLogRedirect(args))
                .UseSerilog((context, services, configuration) =>
                {
                    var options = services.GetRequiredService<Options>();
                    configuration = options.Verbose ? configuration.MinimumLevel.Debug() : configuration.MinimumLevel.Information();

                    configuration = configuration
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.File(
                            options.OutputFile,
                            fileSizeLimitBytes: 5 * 1024 * 1024,
                            retainedFileCountLimit: 2,
                            rollOnFileSizeLimit: true,
                            shared: true,
                            flushToDiskInterval: TimeSpan.FromSeconds(1));
                });
        }
    }
}
