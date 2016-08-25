using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpressiveAggregate.Commands;
using ExpressiveAggregate.Infrastructure;

namespace ExpressiveAggregate
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandRouter = new CommandRouter();
            commandRouter.Route(new ScheduleEvent
            {
                Id = Guid.NewGuid()
            }).Wait();
            Console.ReadKey();
        }
    }
}
