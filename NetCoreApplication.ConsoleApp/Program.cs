using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreApplication.ConsoleApp.Configurations;
using NetCoreApplication.ConsoleApp.Contracts;
using NetCoreApplication.ConsoleApp.RabbitMQ;
using NetCoreApplication.ConsoleApp.Services;
using NLog;
using NLog.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace NetCoreApplication.ConsoleApp
{
    class Program
    {
        public static AppConfig AppConfig { get; set; }

        static async Task Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional:true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.Configure<AppConfig>(hostingContext.Configuration.GetSection("AppConfig"));

                    services.AddMassTransit(cfg =>
                    {
                        //cfg.AddConsumer<TimeConsumer>(t =>
                        //{
                        //});

                        cfg.AddBus(ConfigureBus);
                        //cfg.AddRequestClient<IsItTime>();
                    });

                    services.AddSingleton<IHostedService, MassTransitConsoleHostedService>();
                    //services.AddSingleton<IHostedService, CheckTheTimeService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    logging.AddConsole();
                    logging.AddDebug();
                }).UseNLog();

            await builder.RunConsoleAsync();
        }

        static IBusControl ConfigureBus(IServiceProvider provider)
        {

            AppConfig = provider.GetRequiredService<IOptions<AppConfig>>().Value;

            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(AppConfig.Host, AppConfig.VirtualHost, h =>
                {
                    h.Username(AppConfig.Username);
                    h.Password(AppConfig.Password);
                });

                cfg.ReceiveEndpoint(host, "customer", ec =>
                {
                    ec.UseMessageRetry(r => r.Interval(2, TimeSpan.FromSeconds(5)));
                    ec.Consumer<TimeConsumer>();
                    ec.BindQueue = true;
                    ec.AutoDelete = false;
                    ec.Durable = false;
                });

                //cfg.ConfigureEndpoints(provider);
            });
        }
    }
}
