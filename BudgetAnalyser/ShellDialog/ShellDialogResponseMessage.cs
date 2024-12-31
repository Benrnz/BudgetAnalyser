using Rees.Wpf;

namespace BudgetAnalyser.ShellDialog
{
    public class ShellDialogResponseMessage : MessageBase
    {
        public ShellDialogResponseMessage(object content, ShellDialogButton response)
        {
            Content = content;
            Response = response;
        }

        public object Content { get; private set; }
        public Guid CorrelationId { get; set; }
        public ShellDialogButton Response { get; private set; }

        public bool IsItForMe(Guid correlationId)
        {
            return correlationId != Guid.Empty && correlationId == CorrelationId;
        }
    }
}
