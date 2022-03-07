using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LogRedirect
{
    public static class Program
    {
        public static void Main(string[]? args)
        {
            if (args is null || !args.Any() || args.Any(a => a.Equals("--help", StringComparison.OrdinalIgnoreCase) || a.Equals("-h", StringComparison.OrdinalIgnoreCase)))
            {
                PrintHelp();
                return;
            }

            try
            {
                var parsedCommand = ParseArgs(args);
                Console.WriteLine("Attempting to start: {0}", parsedCommand.FileName);
                if (!string.IsNullOrEmpty(parsedCommand.Arguments))
                {
                    Console.WriteLine("With arguments: {0}", parsedCommand.Arguments);
                }

                ExecuteCommand(parsedCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error: {0} occurred: \"{1}\"", ex.GetType().Name, ex.Message);
                PrintHelp();
            }
        }

        private static void ExecuteCommand(ProcessStartInfo processStartInfo)
        {
            using var process = new Process
            {
                StartInfo = processStartInfo
            };

            process.OutputDataReceived += (_, pArgs) =>
            {
                if (string.IsNullOrEmpty(pArgs.Data))
                {
                    return;
                }

                if (pArgs.Data.Trim().Length <= 2)
                {
                    return;
                }

                Console.WriteLine(pArgs.Data);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Log Redirect");
            Console.WriteLine(string.Empty);
            Console.WriteLine("Usage:");
            Console.WriteLine("  logredirect [options]");
            Console.WriteLine(string.Empty);
            Console.WriteLine("  logredirect --help");
            Console.WriteLine("    This menu");
            Console.WriteLine("  logredirect [executable] [options?]");
            Console.WriteLine("    Run executable with options for that executable and redirect output");
        }

        private static ProcessStartInfo ParseArgs(IEnumerable<string> args)
        {
            var queuedArgs = new Queue<string>(args);
            var executable = queuedArgs.Dequeue();
            if (!File.Exists(executable))
            {
                throw new InvalidOperationException($"Executable not found: {executable}");
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = executable,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(executable),
                WindowStyle = ProcessWindowStyle.Hidden
            };

            if (args.Count() == 1)
            {
                return processStartInfo;
            }

            var arguments = string.Join(' ', queuedArgs);
            processStartInfo.Arguments = arguments;
            return processStartInfo;
        }
    }
}
