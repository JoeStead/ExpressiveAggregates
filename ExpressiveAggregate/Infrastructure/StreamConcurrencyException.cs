using System;

namespace ExpressiveAggregate.Infrastructure
{
    class StreamConcurrencyException : Exception
    {
        public StreamConcurrencyException(string name, int version) : base($"STream: {name} is at {version}")
        {
            
        }
    }
}