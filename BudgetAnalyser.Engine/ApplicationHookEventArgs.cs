using System;

namespace BudgetAnalyser.Engine
{
    public class ApplicationHookEventArgs : EventArgs
    {
        public const string Save = "Save";
        public const string Exit = "Exit";

        public ApplicationHookEventArgs(ApplicationHookEventType eventType, string origin, string subcategory)
        {
            EventType = eventType;
            Origin = origin;
            EventSubCategory = subcategory;
        }

        public ApplicationHookEventType EventType { get; private set; }

        public string EventSubCategory { get; private set; }

        public string Origin { get; private set; }
    }
}