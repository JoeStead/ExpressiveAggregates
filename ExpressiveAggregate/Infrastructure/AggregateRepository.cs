using System;
using System.Collections.Generic;
using System.Linq;
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

    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException(Guid aggregateId, Exception streamNotFoundException) : base ($"Aggregate: {aggregateId} not found", streamNotFoundException)
        {
        }
    }


    class StreamNotFoundException : Exception
    {
        public StreamNotFoundException(string streamName) : base ($"Stream not found for {streamName}")
        {
            
        }
    }

    class StreamConcurrencyException : Exception
    {
        public StreamConcurrencyException(string name, int version) : base($"STream: {name} is at {version}")
        {
            
        }
    }
    public class InMemoryEventStore
    {
        public InMemoryEventStore()
        {
            Streams = new Dictionary<string, ICollection<object>>();
        }

        public IDictionary<string, ICollection<object>> Streams { get; }

        public Task<IEnumerable<object>> LoadEventsForAggregate<TAggregate>(Guid id, int version) where TAggregate : IAggregate
        {
            EnsureValidId(id);
            EnsureValidVersion(version);

            var streamName = GetStreamName<TAggregate>(id);
            if (!Streams.ContainsKey(streamName))
            {
                throw new StreamNotFoundException(streamName);
            }

            var totalEventsToLoad = version;
            var events = Streams[streamName].Take(totalEventsToLoad);

            return Task.FromResult(events);
        }

        public Task SaveEventsForAggregate<TAggregate>(Guid id, int version, IEnumerable<object> events) where TAggregate : IAggregate
        {
            EnsureValidId(id);
            EnsureValidVersion(version);

            var streamName = GetStreamName<TAggregate>(id);
            if (!Streams.ContainsKey(streamName))
            {
                Streams[streamName] = new List<object>();
            }

            var newEvents = events.ToList();
            var originalVersion = version - newEvents.Count;

            if (originalVersion < Streams[streamName].Count)
            {
                throw new StreamConcurrencyException(streamName, originalVersion);
            }

            newEvents.ForEach(x => Streams[streamName].Add(x));

            return Task.FromResult(0);
        }

        public IEnumerable<object> GetEventStreamAt(int position)
        {
            return Streams.Values.ElementAt(position);
        }

        public Task<bool> CheckAggregateExists<TAggregate>(Guid id) where TAggregate : IAggregate
        {
            var streamName = GetStreamName<TAggregate>(id);
            return Task.FromResult(Streams.ContainsKey(streamName));
        }

        private static string GetStreamName<TAggregate>(Guid id)
        {
            var aggregateType = typeof(TAggregate).Name;
            return $"{aggregateType}-{id}";
        }

        private static void EnsureValidId(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new InvalidAggregateIdException(id);
            }
        }

        private static void EnsureValidVersion(int version)
        {
            if (version <= 0)
            {
                throw new InvalidAggregateVersionException(version);
            }
        }
    }

    internal class InvalidAggregateVersionException : Exception
    {
        public InvalidAggregateVersionException(int version) : base ($"Version: {version} is invalid")
        {
        }
    }

    internal class InvalidAggregateIdException : Exception
    {
        public InvalidAggregateIdException(Guid id) : base($"Id cannot be {id}")
        {
        }
    }
}
