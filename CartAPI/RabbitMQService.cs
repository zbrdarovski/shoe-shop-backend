﻿using CartPaymentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CartPaymentAPI
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
                HostName = "your_rabbitmq_hostname",
                Port = your_rabbitmq_port,
                UserName = "your_rabbitmq_username",
                Password = "your_rabbitmq_password"
            };
#else
            _factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "your_rabbitmq_hostname",
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "your_rabbitmq_port"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "your_rabbitmq_username",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "your_rabbitmq_password"
            };
#endif

            Console.WriteLine("hostname: " + Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME"));
            Console.WriteLine("port: " + Environment.GetEnvironmentVariable("RABBITMQ_PORT"));
            Console.WriteLine("username: " + Environment.GetEnvironmentVariable("RABBITMQ_USERNAME"));
            Console.WriteLine("password: " + Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD"));

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare(exchange: "your_rabbitmq_exchange", type: "direct", durable: true);
            _channel.QueueDeclare(queue: "your_rabbitmq_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: "your_rabbitmq_queue", exchange: "your_rabbitmq_exchange", routingKey: "your_rabbitmq_routing_key");
        }

        public void SendLog(LoggingEntry logEntry)
        {
            var json = JsonSerializer.Serialize(logEntry);
            var body = Encoding.UTF8.GetBytes(json);
            _channel.BasicPublish(exchange: "your_rabbitmq_exchange", routingKey: "your_rabbitmq_routing_key", basicProperties: null, body: body);
        }

    }
}
