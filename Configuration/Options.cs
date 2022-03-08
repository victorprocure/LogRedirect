using System;
using System.Collections.Generic;
using CommandLine;

namespace LogRedirect.Configuration
{
    public sealed class Options
    {
        private string? outputFile;
        private string? executable;

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose")]
        public bool Verbose { get; set; }

        [Option('o', "output", Required = true, HelpText = "Set the output file for standard output logging")]
        public string OutputFile { get => outputFile ?? throw new InvalidOperationException($"{nameof(OutputFile)} must be set"); set => outputFile = value; }

        [Option('e', "executable", Required = true, HelpText = "Executable to run and capture standard output")]
        public string Executable { get => executable ?? throw new InvalidOperationException($"{nameof(Executable)} must be set"); set => executable = value; }

        [Value(0)]
        public IEnumerable<string>? ExecutableArguments { get; set; }
    }
}