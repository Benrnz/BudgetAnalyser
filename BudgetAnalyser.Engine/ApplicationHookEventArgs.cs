using System;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// A class to describe event arguments passed when an application event is raised.
    /// </summary>
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

        /// <summary>
        /// An optional sub-category that can be specified to help subscribers respond, or not to the event.
        /// </summary>
        public string EventSubcategory { get; private set; }

        /// <summary>
        /// A major classification for the event.
        /// </summary>
        public ApplicationHookEventType EventType { get; private set; }

        /// <summary>
        /// A string identifier that describes the source of the event. 
        /// </summary>
        public string Origin { get; private set; }
    }
}