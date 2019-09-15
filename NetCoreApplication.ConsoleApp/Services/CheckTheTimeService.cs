using MassTransit;
using Microsoft.Extensions.Hosting;
using NetCoreApplication.ConsoleApp.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreApplication.ConsoleApp.Services
{
    public class CheckTheTimeService : IHostedService
    {
        private readonly IRequestClient<IsItTime> _client;
        private Timer _timer;

        public CheckTheTimeService(IRequestClient<IsItTime> client)
        {
            _client = client;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckTheTime, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose();

            return Task.CompletedTask;
        }

        private async void CheckTheTime(object state)
        {
            var (yes, no) = await _client.GetResponse<YesItIs, NoNotYet>(new { });

            if (yes.IsCompletedSuccessfully)
                await Console.Out.WriteLineAsync("It's party time!");

            if (no.IsCompletedSuccessfully)
                await Console.Out.WriteLineAsync("Nope, not yet");
        }
    }
}
