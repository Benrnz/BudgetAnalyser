using System;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser
{
    public class ShellDialogResponseMessage : MessageBase
    {
        public ShellDialogResponseMessage(object content, ShellDialogResponse response)
        {
            Content = content;
            Response = response;
        }

        public Guid CorrelationId { get; set; }

        public object Content { get; private set; }

        public ShellDialogResponse Response { get; private set; }
    }
}