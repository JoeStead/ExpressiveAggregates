using System;
using System.Threading.Tasks;

namespace ExpressiveAggregate.Infrastructure
{
    public class AggregateRepository
    {
        private readonly InMemoryEventStore _eventStore;

        public AggregateRepository(InMemoryEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public Task<TAggregate> Load<TAggregate>(Guid aggregateId) where TAggregate : class, IAggregate
        {
            return Load<TAggregate>(aggregateId, int.MaxValue);
        }

        public async Task<TAggregate> Load<TAggregate>(Guid aggregateId, int version) where TAggregate : class, IAggregate
        {
            EnsureValidVersion(version);

            try
            {
                var events = await _eventStore.LoadEventsForAggregate<TAggregate>(aggregateId, version);
                var aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate), true);

                foreach (var @event in events)
                {
                    aggregate.ApplyEvent(@event);
                }

                return aggregate;
            }
            catch (StreamNotFoundException ex)
            {
                throw new AggregateNotFoundException(aggregateId, ex);
            }
        }

        public async Task Save<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate
        {

            if (aggregate.Id == Guid.Empty)
            {
                throw new InvalidAggregateIdException(aggregate.Id);
            }

            await _eventStore.SaveEventsForAggregate<TAggregate>(aggregate.Id, aggregate.Version, aggregate.UncommittedEvents);

            aggregate.ClearUncommittedEvents();
        }

        public Task<bool> CheckStreamExists<TAggregate>(Guid id) where TAggregate : IAggregate
        {
            return _eventStore.CheckAggregateExists<TAggregate>(id);
        }

        private static void EnsureValidVersion(int version)
        {
            if (version <= 0)
            {
                throw new InvalidAggregateVersionException(version);
            }
        }
    }
}
