using System;
using System.Threading;
using Docker.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DockerBackup
{
    public static class Startup
    {
        public static void RegisterDependencies(this IServiceCollection services)
        {
            services.AddSingleton<CancellationTokenSource>()
                .AddTransient<IDockerClient>(p => new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient())
                .AddSingleton<App>()
                .AddLogging(configure => configure.AddConsole())                    
                    .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug); // TODO from config
        }
    }
}