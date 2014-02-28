using System;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser
{
    public class RequestShellDialogMessage : MessageBase
    {
        public RequestShellDialogMessage(object content, ShellDialogType type)
        {
            Content = content;
            DialogType = type;
        }

        public Guid CorrelationId { get; set; }

        public object Content { get; private set; }

        public ShellDialogType DialogType { get; private set; }

        public string Title { get; set; }
    }
}