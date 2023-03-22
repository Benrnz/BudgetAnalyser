using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.UserInteraction
{
    /// <summary>
    ///     A Wpf implementation to show a simple dialog that the user should respond Yes or No to.
    /// </summary>
    public class WindowsQuestionBoxYesNo : IUserQuestionBoxYesNo
    {
        /// <summary>
        ///     Show the question box with a caption and heading.
        /// </summary>
        /// <param name="question">The yes/no question to show on the dialog.</param>
        /// <param name="heading">The optional heading for the dialog.</param>
        /// <returns>
        ///     true for yes, false for no, and null for cancellation.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed",
                         Justification = "Optional parameters are prefered unless theres a reason not to.")]
        public bool? Show(string question, string heading = "")
        {
            var result = MessageBox.Show(question, heading, MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    return true;
                case MessageBoxResult.No:
                    return false;
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Show the question box with a caption and heading.
        /// </summary>
        /// <param name="heading">The optional heading for the dialog.</param>
        /// <param name="questionFormat">The yes/no question to show on the dialog.</param>
        /// <param name="argument1">The first argument for the string format.</param>
        /// <param name="args">All other arguments for the string format.</param>
        /// <returns>
        ///     true for yes, false for no, and null for cancellation.
        /// </returns>
        public bool? Show(string heading, string questionFormat, object argument1, params object[] args)
        {
            return Show(string.Format(CultureInfo.CurrentCulture, questionFormat, argument1, args), heading);
        }
    }
}