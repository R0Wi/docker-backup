using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DockerBackup
{
    class Program
    {
        private static bool _shutdown;
        private static readonly object _lockObj = new object();

        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.RegisterDependencies();

            var container = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

            Console.CancelKeyPress += (s, e) => Shutdown(container); // console
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Shutdown(container); // Docker

            await container.GetService<App>().Start();
        }

        private static void Shutdown(IServiceProvider container)
        {
            var logger = container.GetService<ILogger<Program>>();
            logger.LogDebug("Shutdown event received");

            lock (_lockObj)
            {
                if (_shutdown)
                {
                    logger.LogDebug("Shutdown was already done, skipping ...");
                    return;
                }

                logger.LogDebug("Processing cleanup");

                var appCancelTokenSource = container.GetService<CancellationTokenSource>();
                
                appCancelTokenSource.Cancel();
                
                if (container is IDisposable disposable)
                    disposable.Dispose();

                _shutdown = true;
            }
        }
    }
}
