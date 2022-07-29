using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using MassTransit.Logging;
using Microsoft.Extensions.Logging;

namespace GettingStarted
{
    internal class DummyObserver :
        IObserver<KeyValuePair<string, object>>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
        }
    }

    internal class DummyDiagnosticListenerObserver : IObserver<DiagnosticListener>
    {
        IDisposable _handle;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == LogCategoryName.MassTransit)
            {
                _handle = value.Subscribe(new DummyObserver());
            }
        }
    }

    public class Program
    {
        public static Activity globalActivity;
        public static void Main(string[] args)
        {
            DiagnosticListener.AllListeners.Subscribe(new DummyDiagnosticListenerObserver());

            globalActivity = new Activity("global");
            globalActivity.Start();

            var host = CreateHostBuilder(args).Build();
            var bus = host.Services.GetRequiredService<IBusControl>();
            bus.Start();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<MessageConsumer>();

                        x.UsingRabbitMq((context,cfg) =>
                        {
                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddHostedService<Worker>();
                });
    }
}
