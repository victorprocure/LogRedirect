using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LogRedirect.Configuration;
using Microsoft.Extensions.Logging;

namespace LogRedirect
{
    public static class LoggerExtensions
    {
        private readonly static Action<ILogger, string, Exception?> startingApplication = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(StartingApplication)), "Starting application: {executable}");
        private readonly static Action<ILogger, string, IEnumerable<string>, Exception?> startingApplicationWithArguments = LoggerMessage.Define<string, IEnumerable<string>>(LogLevel.Information, new EventId(1, nameof(StartingApplication)), "Starting application: {executable} with arguments: {@arguments}");

        private readonly static Action<ILogger, string, Exception?> executableOutput = LoggerMessage.Define<string>(LogLevel.Information, new EventId(2, nameof(ExecutableOutput)), "{executableOutput}");

        public static void ExecutableOutput(this ILogger logger, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (string.IsNullOrEmpty(dataReceivedEventArgs.Data))
            {
                return;
            }

            if (dataReceivedEventArgs.Data.Trim().Length <= 2)
            {
                return;
            }

            executableOutput(logger, dataReceivedEventArgs.Data, default);
        }

        public static void StartingApplication(this ILogger logger, Options options)
        {
            if (options.ExecutableArguments?.Any() == true)
            {
                startingApplicationWithArguments(logger, options.Executable, options.ExecutableArguments, default);
            }
            else
            {
                startingApplication(logger, options.Executable, default);
            }
        }
    }
}