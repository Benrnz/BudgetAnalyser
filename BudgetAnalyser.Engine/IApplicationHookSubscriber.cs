using System.Collections;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine
{
    public interface IApplicationHookSubscriber
    {
        void Subscribe(IEnumerable<IApplicationHookEventPublisher> publishers);
    }
}