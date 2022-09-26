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
            

            globalActivity = new Activity("global");
            globalActivity.Start();
            Console.WriteLine($"Root activity: {globalActivity.Id}");

            IHost host = CreateHostBuilder(args).Build();

            // This worked in v7, but no longer works in v8.
            // DiagnosticListener.AllListeners.Subscribe(new DummyDiagnosticListenerObserver());
            // This enables child activity creation in MT v8.
            using var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStarted = activity => {},
                ActivityStopped = activity => {}
            };
            ActivitySource.AddActivityListener(listener);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<MessageConsumer>();
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.UseNewtonsoftJsonSerializer();
                            cfg.UseNewtonsoftJsonDeserializer();
                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddHostedService<Worker>();
                });
    }
}
