using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Reciever.Binders;
using Reciever.Broker.Client.Interface;
using Reciever.Broker.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reciever.Broker.Client.Service
{
    public class ConsumerService : IConsumerService
    {
        private ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly BrokerConfig _brokerConfig;


        public ConsumerService(ILogger<BrokerDaemon> loggerFactory, IOptionsSnapshot<BrokerConfig> config)
        {
            _logger = loggerFactory;
            _brokerConfig = config.Value;
            InitRabbitMQ();
        }

        public void InitRabbitMQ()
        {
            var server = _brokerConfig.Server;
            var factory = new ConnectionFactory();
            
            if (server.isLocal)
            {
                factory = new ConnectionFactory { HostName = server.IP };
            }
            else
            {
                factory = new ConnectionFactory { HostName = server.IP, UserName = server.Username, Password = server.Password };
            }

            // create connection
            _connection = factory.CreateConnection();

            // create channel
            _channel = _connection.CreateModel();

            //declare exchange to be used for message delivery
            _channel.ExchangeDeclare(server.Exchange, ExchangeType.Direct);

            //declare queue where message will be recieved
            _channel.QueueDeclare(_brokerConfig.ClientQueue.QueueId, false, false, false, null);

            //Bind queue to use defined exchange
            _channel.QueueBind(_brokerConfig.ClientQueue.QueueId, server.Exchange, _brokerConfig.ClientQueue.RoutingKey, null);
            _channel.BasicQos(0, _brokerConfig.ClientQueue.MaxQueueCount, false);

            //Set Event to be triggered on connection shutdown
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"connection shut down {e.ReplyText}");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation($"consumer cancelled {e.ConsumerTag}");
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation($"consumer unregistered {e.ConsumerTag}");
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation($"consumer registered!");
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"consumer shutdown {e.ReplyText}");
        }

        public Task StartListeningForPayload()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message
                try
                {
                    var content = System.Text.Encoding.UTF8.GetString(ea.Body);

                    // handle the received message
                    HandleMessage(content, ea);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"RabbitMQ: Exception while recieving payload with tag {ea.DeliveryTag} => {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, true, true);
                }

            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(_brokerConfig.ClientQueue.QueueId, false, consumer);
            _logger.LogInformation("Actively listening for incoming payload");

            return Task.CompletedTask;
        }

        private void HandleMessage(string content, BasicDeliverEventArgs eventProperties)
        {
            var header = eventProperties.BasicProperties.Headers;

            header.TryGetValue("eventId", out dynamic eventIdByteValue);
            if (eventIdByteValue == null) return;
            string eventId = Encoding.UTF8.GetString(eventIdByteValue);

            // Determine action from event ID
            switch (eventId)
            {
                case BrokerEvents.NotifyClient:

                    // Print recieved message on console
                    _logger.LogInformation($"received: {content}");
                    break;
                default:
                    // Print error message on console
                    _logger.LogInformation($"Recieved content does not match configured event ID");
                    break;

            }
        }

        public void CloseBrokerConnection()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
