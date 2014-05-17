using System;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.ShellDialog
{
    public class ShellDialogRequestMessage : MessageBase
    {
        public ShellDialogRequestMessage(BudgetAnalyserFeature location, object content, ShellDialogType type)
        {
            Location = location;
            Content = content;
            DialogType = type;
        }

        /// <summary>
        ///     The view model object that contains the content. A data template will be sought out for this type.
        /// </summary>
        public object Content { get; private set; }

        /// <summary>
        ///     A correlation id used for when the pop up is resolved and a call back is made via the
        ///     <see cref="ShellDialogResponseMessage" />.
        /// </summary>
        public Guid CorrelationId { get; set; }

        public ShellDialogType DialogType { get; private set; }
        public BudgetAnalyserFeature Location { get; private set; }

        public string Title { get; set; }
    }
}