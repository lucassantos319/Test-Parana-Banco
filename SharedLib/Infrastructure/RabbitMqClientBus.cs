using SharedLib.Domain.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedLib.Domain.Interfaces.Bus;
using SharedLib.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace SharedLib.Infrastructure
{
    public sealed class RabbitMqClientBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers; 
        private readonly List<Type> _eventTypes;
        private readonly IServiceProvider _serviceProvider;
        /*private readonly IConfiguration _configuration;*/

        public RabbitMqClientBus(IMediator mediator, IServiceProvider serviceProvider)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }
        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory() { HostName = "localhost"};
            using var connection = factory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                var eventName = @event.GetType().Name;

                channel.QueueDeclare(queue: eventName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var message = JsonSerializer.Serialize(@event);
                var body = System.Text.Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: eventName,
                                     basicProperties: null,
                                     body: body);
            }
        }


        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if ( !_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers[eventName] = new List<Type>();
            }

            if (_handlers[eventName].Any(x => x.GetType() == handlerType))
            {
                throw new ArgumentException($"Handler {handlerType.Name} already registered for {eventName}",nameof(handlerType));
            }

            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory() 
            { 
                HostName = "localhost",
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var eventName = typeof(T).Name;
            channel.QueueDeclare(queue: eventName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(queue: eventName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var message = System.Text.Encoding.UTF8.GetString(@event.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error processing message: {ex.Message}");
                return;
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            Console.WriteLine($"Processing event: {eventName}");
            Console.WriteLine($"Message: {message}");
            
            if ( _handlers.ContainsKey(eventName) )
            {
                var subscriptions = _handlers[eventName];
                Console.WriteLine($"Found {subscriptions.Count} handlers for event {eventName}");
                
                foreach (var subscription in subscriptions)
                {
                    Console.WriteLine($"Creating handler instance for type: {subscription.Name}");
                    var handler = _serviceProvider.GetService(subscription);
                    if (handler == null) 
                    {
                        Console.WriteLine($"Failed to create handler instance for type: {subscription.Name}");
                        continue;
                    }

                    var eventType = _eventTypes.FirstOrDefault(x => x.Name == eventName);
                    if (eventType == null) 
                    {
                        Console.WriteLine($"Event type not found for event: {eventName}");
                        continue;
                    }
                    
                    Console.WriteLine($"Deserializing event to type: {eventType.Name}");
                    var @event = JsonSerializer.Deserialize(message, eventType);
                    if (@event == null) 
                    {
                        Console.WriteLine($"Failed to deserialize event");
                        continue;
                    }
                    
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    Console.WriteLine($"Invoking Handle method on handler: {handler.GetType().Name}");
                    
                    try
                    {
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                        Console.WriteLine($"Successfully processed event {eventName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error invoking handler: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"No handlers registered for event: {eventName}");
                Console.WriteLine($"Available handlers: {string.Join(", ", _handlers.Keys)}");
            }

        }
    }
}
