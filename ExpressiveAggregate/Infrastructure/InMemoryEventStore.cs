using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressiveAggregate.Infrastructure
{
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
}