using System;

namespace ExpressiveAggregate.Infrastructure
{
    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException(Guid aggregateId, Exception streamNotFoundException) : base ($"Aggregate: {aggregateId} not found", streamNotFoundException)
        {
        }
    }
}