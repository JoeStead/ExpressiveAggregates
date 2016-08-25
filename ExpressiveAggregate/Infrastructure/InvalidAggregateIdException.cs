using System;

namespace ExpressiveAggregate.Infrastructure
{
    internal class InvalidAggregateIdException : Exception
    {
        public InvalidAggregateIdException(Guid id) : base($"Id cannot be {id}")
        {
        }
    }
}