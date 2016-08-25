using System;

namespace ExpressiveAggregate.Infrastructure
{
    class StreamNotFoundException : Exception
    {
        public StreamNotFoundException(string streamName) : base ($"Stream not found for {streamName}")
        {
            
        }
    }
}