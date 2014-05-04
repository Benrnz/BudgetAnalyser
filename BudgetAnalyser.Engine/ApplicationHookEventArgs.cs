using System;

namespace BudgetAnalyser.Engine
{
    public class ApplicationHookEventArgs : EventArgs
    {
        public const string Exit = "Exit";
        public const string Save = "Save";

        public ApplicationHookEventArgs(ApplicationHookEventType eventType, string origin, string subcategory)
        {
            EventType = eventType;
            Origin = origin;
            EventSubcategory = subcategory;
        }

        public string EventSubcategory { get; private set; }
        public ApplicationHookEventType EventType { get; private set; }

        public string Origin { get; private set; }
    }
}