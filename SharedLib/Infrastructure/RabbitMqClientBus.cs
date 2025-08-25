using SharedLib.Domain.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedLib.Domain.Interfaces.Bus;
using SharedLib.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SharedLib.Infrastructure
{
    public sealed class RabbitMqClientBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqClientBus> _logger;
        private readonly Dictionary<string, int> _retryCounts;
        private const int MaxRetries = 5;

        public RabbitMqClientBus(IMediator mediator, IServiceProvider serviceProvider, ILogger<RabbitMqClientBus> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
            _retryCounts = new Dictionary<string, int>();
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }
        public void Publish<T>(T @event) where T : Event
        {
            try
            {
                _logger.LogInformation("Publishing event: {EventType} with correlation ID: {CorrelationId}",
                    typeof(T).Name, @event.CorrelationId);

                var factory = new ConnectionFactory() { HostName = "localhost" };
                using var connection = factory.CreateConnection();
                using (var channel = connection.CreateModel())
                {
                    var eventName = @event.GetType().Name;
                    var routingKey = eventName.ToLower().Replace("event", "");

                    _logger.LogInformation("Publishing to exchange: bank.events, routing key: {RoutingKey}, queue: {QueueName}",
                        routingKey, eventName);

                    // Declarar exchange
                    channel.ExchangeDeclare("bank.events", ExchangeType.Topic, durable: true);

                    // Declarar fila principal
                    channel.QueueDeclare(queue: eventName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    // Declarar Dead Letter Queue
                    channel.QueueDeclare(queue: $"{eventName}.dlq",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var message = JsonSerializer.Serialize(@event, new JsonSerializerOptions
                    {
                        WriteIndented = false
                    });
                    var body = System.Text.Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.MessageId = @event.CorrelationId.ToString();
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                    channel.BasicPublish(exchange: "bank.events",
                                         routingKey: routingKey,
                                         basicProperties: properties,
                                         body: body);

                    _logger.LogInformation("Successfully published event {EventType} with correlation ID: {CorrelationId}",
                        eventName, @event.CorrelationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event {EventType} with correlation ID: {CorrelationId}",
                    typeof(T).Name, @event.CorrelationId);
                throw;
            }
        }


        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            _logger.LogInformation("Subscribing handler {HandlerType} for event {EventName}", handlerType.Name, eventName);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
                _logger.LogInformation("Added event type {EventType} to event types", typeof(T).Name);
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers[eventName] = new List<Type>();
                _logger.LogInformation("Created new handler list for event {EventName}", eventName);
            }

            if (_handlers[eventName].Any(x => x == handlerType))
            {
                throw new ArgumentException($"Handler {handlerType.Name} already registered for {eventName}", nameof(handlerType));
            }

            _handlers[eventName].Add(handlerType);
            _logger.LogInformation("Added handler {HandlerType} to event {EventName}. Total handlers: {HandlerCount}",
                handlerType.Name, eventName, _handlers[eventName].Count);

            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            try
            {
                _logger.LogInformation("Starting consumer for event: {EventType}", typeof(T).Name);

                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    DispatchConsumersAsync = true
                };

                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();

                var eventName = typeof(T).Name;
                var routingKey = eventName.ToLower().Replace("event", "");
                _logger.LogInformation("Declaring queue: {QueueName} with routing key: {RoutingKey}", eventName, routingKey);

                // Declarar exchange
                channel.ExchangeDeclare("bank.events", ExchangeType.Topic, durable: true);

                // Declarar fila principal
                channel.QueueDeclare(queue: eventName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                // Binding
                channel.QueueBind(eventName, "bank.events", routingKey);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;

                // Configurar QoS para processamento controlado
                channel.BasicQos(0, 1, false);

                channel.BasicConsume(queue: eventName,
                                     autoAck: false, // Manual ACK para controle de retry
                                     consumer: consumer);

                _logger.LogInformation("Successfully started consumer for event: {EventType}", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting consumer for event {EventType}", typeof(T).Name);
                throw;
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var channel = ((AsyncEventingBasicConsumer)sender).Model;
            var routingKey = @event.RoutingKey;
            var message = System.Text.Encoding.UTF8.GetString(@event.Body.ToArray());
            var correlationId = @event.BasicProperties.MessageId ?? Guid.NewGuid().ToString();

            // Converter routing key para nome da classe do evento
            var eventName = ConvertRoutingKeyToEventName(routingKey);

            _logger.LogInformation("Received event: {EventName} (routing key: {RoutingKey}) with correlation ID: {CorrelationId}", 
                eventName, routingKey, correlationId);

            try
            {
                await ProcessEvent(eventName, message, channel, @event.DeliveryTag).ConfigureAwait(false);
                channel.BasicAck(@event.DeliveryTag, false);
                _logger.LogInformation("Successfully processed event: {EventName} with correlation ID: {CorrelationId}", eventName, correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event: {EventName} with correlation ID: {CorrelationId}", eventName, correlationId);

                // Implementar retry logic
                var retryCount = GetRetryCount(correlationId);
                if (retryCount < MaxRetries)
                {
                    _logger.LogWarning("Retrying event: {EventName} with correlation ID: {CorrelationId}. Retry count: {RetryCount}",
                        eventName, correlationId, retryCount);

                    // Rejeitar mensagem para retry
                    channel.BasicNack(@event.DeliveryTag, false, true);
                    IncrementRetryCount(correlationId);
                }
                else
                {
                    _logger.LogError("Max retries exceeded for event: {EventName} with correlation ID: {CorrelationId}. Moving to DLQ.",
                        eventName, correlationId);

                    // Publicar evento de falha
                    await PublishFailureEvent(eventName, message, ex.Message, retryCount, correlationId);

                    // Aceitar mensagem para remover da fila
                    channel.BasicAck(@event.DeliveryTag, false);
                    RemoveRetryCount(correlationId);
                }
            }
        }

        private async Task ProcessEvent(string eventName, string message, IModel channel, ulong deliveryTag)
        {
            _logger.LogInformation("Processing event: {EventName}", eventName);

            if (_handlers.ContainsKey(eventName))
            {
                var subscriptions = _handlers[eventName];
                _logger.LogInformation("Found {HandlerCount} handlers for event {EventName}", subscriptions.Count, eventName);

                foreach (var subscription in subscriptions)
                {
                    _logger.LogInformation("Creating handler instance for type: {HandlerType}", subscription.Name);
                    var handler = _serviceProvider.GetService(subscription);
                    if (handler == null)
                    {
                        _logger.LogError("Failed to create handler instance for type: {HandlerType}", subscription.Name);
                        continue;
                    }

                    var eventType = _eventTypes.FirstOrDefault(x => x.Name == eventName);
                    if (eventType == null)
                    {
                        _logger.LogError("Event type not found for event: {EventName}", eventName);
                        continue;
                    }

                    _logger.LogInformation("Deserializing event to type: {EventType}", eventType.Name);
                    try
                    {
                        var @event = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (@event == null)
                        {
                            _logger.LogError("Failed to deserialize event: {EventName}", eventName);
                            continue;
                        }

                        _logger.LogInformation("Successfully deserialized event: {EventType}", eventType.Name);

                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                        _logger.LogInformation("Invoking Handle method on handler: {HandlerType}", handler.GetType().Name);

                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                        _logger.LogInformation("Successfully processed event {EventName}", eventName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during deserialization or processing of event: {EventName}", eventName);
                        throw;
                    }
                }
            }
            else
            {
                _logger.LogWarning("No handlers registered for event: {EventName}. Available handlers: {AvailableHandlers}",
                    eventName, string.Join(", ", _handlers.Keys));
            }
        }

        private int GetRetryCount(string correlationId)
        {
            return _retryCounts.TryGetValue(correlationId, out var count) ? count : 0;
        }

        private void IncrementRetryCount(string correlationId)
        {
            if (_retryCounts.ContainsKey(correlationId))
            {
                _retryCounts[correlationId]++;
            }
            else
            {
                _retryCounts[correlationId] = 1;
            }
        }

        private void RemoveRetryCount(string correlationId)
        {
            _retryCounts.Remove(correlationId);
        }

        private async Task PublishFailureEvent(string eventName, string originalMessage, string errorMessage, int retryCount, string correlationId)
        {
            try
            {
                var failureEvent = new ProcessFailureEvent(
                    Guid.Parse(correlationId), // Assumindo que correlationId é um GUID
                    "RabbitMqClientBus",
                    errorMessage,
                    retryCount,
                    eventName,
                    originalMessage
                );

                Publish(failureEvent);
                _logger.LogInformation("Published failure event for correlation ID: {CorrelationId}", correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish failure event for correlation ID: {CorrelationId}", correlationId);
            }
        }

        private string ConvertRoutingKeyToEventName(string routingKey)
        {
            // Mapeamento de routing keys para nomes de eventos
            var eventNameMappings = new Dictionary<string, string>
            {
                { "clientcreated", "ClientCreatedEvent" },
                { "creditproposalrequest", "CreditProposalRequestEvent" },
                { "creditproposalresult", "CreditProposalResultEvent" },
                { "creditcardrequest", "CreditCardRequestEvent" },
                { "creditcardresult", "CreditCardResultEvent" },
                { "processfailure", "ProcessFailureEvent" }
            };

            if (eventNameMappings.TryGetValue(routingKey.ToLower(), out var eventName))
            {
                return eventName;
            }

            // Fallback: tentar converter automaticamente
            var words = routingKey.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            var pascalCase = string.Join("", words.Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
            return pascalCase + "Event";
        }
    }
}

