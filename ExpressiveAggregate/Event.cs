using ExpressiveAggregate.Commands;
using ExpressiveAggregate.Events;
using ExpressiveAggregate.Infrastructure;

namespace ExpressiveAggregate
{
    class Event : Aggregate<Event>
    {
        static Event()
        {
            Given<EventScheduled>((a, e) =>
            {
                a.Id = e.Id;
            });
        }

        public Event(ScheduleEvent command)
        {
            Then(new EventScheduled(command.Id));
        }
    }
}