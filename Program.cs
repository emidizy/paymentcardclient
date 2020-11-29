﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Reciever.Broker.Client.Interface;
using Reciever.Broker.Client.Service;
using Reciever.Binders;
using System.IO;

namespace Reciever
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, this is the Consumer application!");
            var builder = new HostBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
               
                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();

                //Register appsetting bindings
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                    .AddJsonFile("appsettings.json", false)
                    .Build();

                services.Configure<BrokerConfig>(configuration.GetSection("BrokerConfig"));

                //Register core services
                services.AddSingleton<IConsumerService, ConsumerService>();
                services.AddSingleton<IHostedService, BrokerDaemon>();
            })
            .ConfigureLogging((hostingContext, logging) => {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
            });

            await builder.RunConsoleAsync();
            
        }

       
    }
}
