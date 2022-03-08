using System;
using System.Collections.Generic;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogRedirect.Configuration
{
    internal static class ServiceRegistration
    {

        public static IServiceCollection AddLogRedirect(this IServiceCollection services, IEnumerable<string>? args)
        {
            services.AddSingleton<Options>(sd =>
            {
                var parser = new Parser(x =>
                {
                    x.EnableDashDash = true;
                });

                Options? options = default;
                parser.ParseArguments<Options>(args).WithParsed(o => options = o);

                return options ?? throw new InvalidOperationException("Unable to parse options");
            });
            services.AddHostedService<LogRedirectService>();

            return services;
        }
    }
}