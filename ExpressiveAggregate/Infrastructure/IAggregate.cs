using System;
using System.Collections.Generic;

namespace ExpressiveAggregate.Infrastructure
{
    public interface IAggregate
    {
        Guid Id { get; }
        int Version { get;  }
        IEnumerable<object> UncommittedEvents { get; }
        void ApplyEvent<TEvent>(TEvent @event);
        void ClearUncommittedEvents();
    }
}