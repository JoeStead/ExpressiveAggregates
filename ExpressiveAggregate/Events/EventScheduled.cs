using System;

namespace ExpressiveAggregate.Events
{
    class EventScheduled
    {
        public EventScheduled(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}