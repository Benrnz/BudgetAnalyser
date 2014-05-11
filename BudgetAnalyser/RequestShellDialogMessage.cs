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

        /// <summary>
        /// A correlation id used for when the pop up is resolved and a call back is made via the <see cref="ShellDialogResponseMessage"/>.
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// The view model object that contains the content. A data template will be sought out for this type.
        /// </summary>
        public object Content { get; private set; }

        public ShellDialogType DialogType { get; private set; }

        public string Title { get; set; }
    }
}