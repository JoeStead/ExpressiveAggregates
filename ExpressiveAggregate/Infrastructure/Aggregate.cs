using System;
using System.Collections.Generic;

namespace ExpressiveAggregate.Infrastructure
{
    abstract class Aggregate<T> : IAggregate where T : Aggregate<T>
    {
        public Guid Id { get; protected set; }
        int IAggregate.Version => _version;

        private static readonly Dictionary<Type, object> EventActions = new Dictionary<Type, object>();
        private List<object> _uncommittedEvents = new List<object>();
        private int _version;
        protected static void Given<TEvent>(Action<T, TEvent> action) where TEvent : class
        {
            EventActions.Add(typeof(TEvent), action);
        }

        protected void Then<TEvent>(TEvent @event)
        {
            _uncommittedEvents.Add(@event);
            ApplyEvent(this, @event);
        }

        private void ApplyEvent<TEvent>(IAggregate aggregate, TEvent @event)
        {
            aggregate.ApplyEvent(@event);
        }

        IEnumerable<object> IAggregate.UncommittedEvents => _uncommittedEvents;


        void IAggregate.ClearUncommittedEvents()
        {
            _uncommittedEvents.Clear();
        }

        void IAggregate.ApplyEvent<TEvent>(TEvent @event)
        {
            ((Action<T, TEvent>)EventActions[typeof(TEvent)]).Invoke((T)this, @event);
            _version++;
        }
    }
}