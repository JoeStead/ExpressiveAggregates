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
            var aggregateMethods = new Dictionary<Type, Type>();
            var constructorCommands = new Dictionary<Type,Type>();
            var aggregates = Assembly.GetAssembly(typeof(Program)).DefinedTypes.Where(t => t.ImplementedInterfaces.Any(i => i == typeof(IAggregate) && !t.IsAbstract));
            foreach (var aggregate in aggregates)
            {
                var whenMethods = aggregate.DeclaredMethods.Where(m => m.Name == "When");
                var constructor = aggregate.DeclaredConstructors.SingleOrDefault(c => c.GetParameters().Length == 1)?.GetParameters().Single();
                if (constructor != null)
                {
                    constructorCommands.Add(constructor.ParameterType, aggregate);
                }

                foreach (var whenMethod in whenMethods)
                {
                    var command = whenMethod.GetParameters().First().ParameterType;
                    aggregateMethods.Add(command, aggregate);
                }
            }
            var @event = new Event(new ScheduleEvent
            {
                Id = Guid.NewGuid()
            });
            Console.WriteLine(@event.Id);
            Console.ReadKey();
        }
    }
}
