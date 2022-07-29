using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GettingStarted
{
    public class Worker : BackgroundService
    {
        readonly IBus _bus;
        readonly ILogger<Worker> _logger;

        public Worker(IBus bus, ILogger<Worker> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _work;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _work = Task.Run(() => DoWork(_cts.Token));
            return Task.CompletedTask;
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                await _bus.Publish(new Message { Text = $"The time is {DateTimeOffset.Now}" }, stoppingToken);

                await Task.Delay(1000, stoppingToken);
            }
            _logger.LogInformation("Worker stopped.");
        }
        
        

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _cts.Cancel();
            try
            {
                _work.Wait(1000, cancellationToken);
            }
            catch (AggregateException ae) when (ae.InnerExceptions[0] is TaskCanceledException)
            {
                _logger.LogInformation("Worker cancelled.");
            }
        }
    }
}
