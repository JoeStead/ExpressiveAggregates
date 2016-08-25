using System;

namespace ExpressiveAggregate.Infrastructure
{
    internal class InvalidAggregateVersionException : Exception
    {
        public InvalidAggregateVersionException(int version) : base ($"Version: {version} is invalid")
        {
        }
    }
}