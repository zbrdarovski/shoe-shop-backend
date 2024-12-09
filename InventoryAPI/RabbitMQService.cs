using InventoryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace InventoryAPI
{
    public class RabbitMQService
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQService()
        {

#if DEBUG
            _factory = new ConnectionFactory()
            {
                HostName = "rabbitmq_hostname",
                Port = rabbitmq_port,
                UserName = "rabbitmq_username",
                Password = "rabbitmq_password"
            };
#else
            _factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "rabbitmq_hostname",
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "rabbitmq_port"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "rabbitmq_username",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "rabbitmq_password"
            };
#endif

            Console.WriteLine("hostname: " + Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME"));
            Console.WriteLine("port: " + Environment.GetEnvironmentVariable("RABBITMQ_PORT"));
            Console.WriteLine("username: " + Environment.GetEnvironmentVariable("RABBITMQ_USERNAME"));
            Console.WriteLine("password: " + Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD"));

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare(exchange: "rabbitmq_exchange", type: "direct", durable: true);
            _channel.QueueDeclare(queue: "rabbitmq_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: "rabbbitmq_queue", exchange: "rabbitmq_exchange", routingKey: "rabbitmq_routingkey");
        }

        public void SendLog(LoggingEntry logEntry)
        {
            var json = JsonSerializer.Serialize(logEntry);
            var body = Encoding.UTF8.GetBytes(json);
            _channel.BasicPublish(exchange: "rabbitmq_exchange", routingKey: "rabbitmq_routingkey", basicProperties: null, body: body);
        }

    }
}
