using ExpressiveAggregate.Commands;
using ExpressiveAggregate.Infrastructure;

namespace ExpressiveAggregate
{
    class Order : Aggregate<Order>
    {
        public void When(PurchaseOrder command)
        {
            Then<object>(null);
        }
    }
}