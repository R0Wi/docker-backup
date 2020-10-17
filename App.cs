using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DockerBackup
{
    public class App : IDisposable
    {
        private readonly IDockerClient _client;
        private readonly CancellationTokenSource _appCancellationTokenSource;
        private readonly ILogger<App> _logger;

        public App(IDockerClient client, ILogger<App> logger, CancellationTokenSource appCancellationTokenSource)
        {
            _client = client;
            _appCancellationTokenSource = appCancellationTokenSource;
            _logger = logger;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task Start()
        {
            var progress = new Progress<Message>();
            progress.ProgressChanged += async (s, e) =>
            {
                _logger.LogDebug($"--- Event: ---{Environment.NewLine}{JsonConvert.SerializeObject(e, Formatting.Indented)}");
                if (e.Type == "container")
                {
                    if (e.Action == "start")
                    {
                        try
                        {
                            var inspectResult = await _client.Containers.InspectContainerAsync(e.Actor.ID, _appCancellationTokenSource.Token);
                            var ip = inspectResult.NetworkSettings.IPAddress;
                            var env = inspectResult.Config.Env;

                            _logger.LogDebug($"--- Inspect: ---{Environment.NewLine}{JsonConvert.SerializeObject(inspectResult, Formatting.Indented)}");
                        }
                        catch (DockerContainerNotFoundException)
                        {
                            return;
                        }
                    }
                }
            };
            var filters = new Dictionary<string, IDictionary<string, bool>>();
            filters["event"] = new Dictionary<string, bool> { { "start", true }, { "die", true } };
            await _client.System.MonitorEventsAsync(new ContainerEventsParameters { Filters = filters, Since = DateTime.Now.TimeOfDay.Ticks.ToString() }, progress, _appCancellationTokenSource.Token);
        }
    }
}