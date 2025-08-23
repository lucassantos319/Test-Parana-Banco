using SharedLib.Domain.Entities;

namespace SharedLib.Domain.Interfaces.Bus;

public interface IEventHandler<in TEvent> : IEventHandler 
    where TEvent : Event 
{
    Task Handle(TEvent @event);
}

public interface IEventHandler 
{
}


