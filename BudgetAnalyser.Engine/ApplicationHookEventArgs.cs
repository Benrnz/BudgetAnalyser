using System;

namespace BudgetAnalyser.Engine
{
    public class ApplicationHookEventArgs : EventArgs
    {
        public ApplicationHookEventArgs(ApplicationHookEventType eventType, string origin)
        {
            EventType = eventType;
            Origin = origin;
        }

        public ApplicationHookEventType EventType { get; private set; }

        public string Origin { get; private set; }
    }
}