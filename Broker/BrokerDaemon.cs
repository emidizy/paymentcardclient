using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Reciever.Binders;
using Reciever.Broker.Client.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reciever
{
    public class BrokerDaemon : IHostedService, IDisposable
    {
        private ILogger _logger;
        private IConsumerService _consumerSvc;
        private readonly BrokerConfig _brokerConfig;


        public BrokerDaemon(ILogger<BrokerDaemon> loggerFactory, 
            IOptionsSnapshot<BrokerConfig> config,
            IConsumerService consumerService)
        {
            _logger = loggerFactory;
            _brokerConfig = config.Value;
            _consumerSvc = consumerService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting daemon: " + _brokerConfig.ServiceName);
            return _consumerSvc.StartListeningForPayload();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping daemon.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogInformation("Closing connection...");
            _consumerSvc.CloseBrokerConnection();
        }
    }
}
