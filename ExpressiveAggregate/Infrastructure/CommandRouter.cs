using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ExpressiveAggregate.Infrastructure
{
    class CommandRouter
    {
        private readonly Dictionary<Type, Type> _aggregateMethods;
        private readonly Dictionary<Type, Type> _constructorCommands;
        private readonly AggregateRepository _aggregateRepository;

        public CommandRouter()
        {
            _aggregateMethods = new Dictionary<Type, Type>();
            _constructorCommands = new Dictionary<Type, Type>();
            _aggregateRepository = new AggregateRepository(new InMemoryEventStore());
            DiscoverRoutes();
        }

        private void DiscoverRoutes()
        {
            var aggregates = Assembly.GetAssembly(typeof(Program)).DefinedTypes.Where(t => t.ImplementedInterfaces.Any(i => i == typeof(IAggregate) && !t.IsAbstract));
            foreach (var aggregate in aggregates)
            {
                var whenMethods = aggregate.DeclaredMethods.Where(m => m.Name == "When");
                var constructor = aggregate.DeclaredConstructors.SingleOrDefault(c => c.GetParameters().Length == 1)?.GetParameters().Single();
                if (constructor != null)
                {
                    _constructorCommands.Add(constructor.ParameterType, aggregate);
                }

                foreach (var whenMethod in whenMethods)
                {
                    var command = whenMethod.GetParameters().First().ParameterType;
                    _aggregateMethods.Add(command, aggregate);
                }
            }

        }

        public async Task Route<TCommand>(TCommand command)
        {
            var commandType = typeof(TCommand);
            if (_constructorCommands.ContainsKey(commandType))
            {
                var aggregateType = _constructorCommands[commandType];
                var aggregate = (IAggregate) Activator.CreateInstance(aggregateType, command);
                await _aggregateRepository.Save(aggregate);
            }
        }
    }
}
