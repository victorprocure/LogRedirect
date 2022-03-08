using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LogRedirect.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogRedirect
{
    public sealed class LogRedirectService : IHostedService, IDisposable
    {
        private readonly Options options;
        private readonly IHostApplicationLifetime appLifeTime;
        private readonly ILogger<LogRedirectService> logger;
        private Process? externalApplicationProcess;

        private int? exitCode;

        public LogRedirectService(Options options, IHostApplicationLifetime appLifeTime, ILogger<LogRedirectService> logger)
        {
            this.options = options;
            this.appLifeTime = appLifeTime;
            this.logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.StartingApplication(options);
            appLifeTime.ApplicationStarted.Register(() =>
                Task.Run(async () =>
                {
                    try
                    {
                        var processStartInfo = ParseArgs(options);
                        await ExecuteCommandAsync(processStartInfo);
                        exitCode = 0;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unhandled exception");
                        exitCode = 1;
                    }
                    finally
                    {
                        appLifeTime.StopApplication();
                    }
                })
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping LogRedirect");
            Environment.ExitCode = exitCode.GetValueOrDefault(-1);
            externalApplicationProcess?.CloseMainWindow();

            return Task.CompletedTask;
        }

        private async Task ExecuteCommandAsync(ProcessStartInfo processStartInfo)
        {
            externalApplicationProcess = new Process
            {
                StartInfo = processStartInfo
            };

            externalApplicationProcess.OutputDataReceived += OnOutputDataReceived;

            externalApplicationProcess.Start();
            externalApplicationProcess.BeginOutputReadLine();
            await externalApplicationProcess.WaitForExitAsync();
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
            => logger.ExecutableOutput(dataReceivedEventArgs);

        private static ProcessStartInfo ParseArgs(Options options)
        {
            if (!File.Exists(options.Executable))
            {
                throw new InvalidOperationException($"Executable not found: {options.Executable}");
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = options.Executable,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(options.Executable),
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var arguments = string.Join(' ', options.ExecutableArguments ?? Array.Empty<string>());
            processStartInfo.Arguments = arguments;
            return processStartInfo;
        }

        public void Dispose()
        {
            externalApplicationProcess?.Dispose();
        }
    }
}