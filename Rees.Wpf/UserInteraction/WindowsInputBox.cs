using System;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.UserInteraction
{
    /// <summary>
    ///     A Wpf implementation of a simple Windows Input box that prompts the user to enter some text with an Ok and Cancel
    ///     button.
    /// </summary>
    public class WindowsInputBox : IUserInputBox
    {
        private readonly IViewLoader viewLoader;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowsInputBox" /> class.
        /// </summary>
        /// <param name="viewLoader">The view loader that is used to show the input box.</param>
        /// <exception cref="System.ArgumentNullException">viewLoader cannot be null.</exception>
        public WindowsInputBox(IViewLoader viewLoader)
        {
            if (viewLoader == null)
            {
                throw new ArgumentNullException("viewLoader");
            }

            this.viewLoader = viewLoader;
        }

        /// <summary>
        ///     Gets or sets the heading for the input box.
        /// </summary>
        public string Heading { get; set; }

        /// <summary>
        ///     Gets or sets the input. Setting this before display, will default the text. After the user has clicked Ok, this
        ///     will contain the user's text.
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        ///     Gets or sets the question.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        ///     Show an input box with a heading and a question.
        /// </summary>
        /// <param name="heading">The window title</param>
        /// <param name="question">The main question text</param>
        /// <param name="defaultInput">An optional default value for the input box</param>
        /// <returns>
        ///     User response to the question, or null if the user cancelled.  The user cannot return null if 'Ok' is clicked.
        /// </returns>
        public string Show(string heading, string question, string defaultInput = "")
        {
            Heading = heading;
            Question = question;
            Input = defaultInput;
            var result = this.viewLoader.ShowDialog(this);

            if (result == null || result == false)
            {
                Input = null;
                return null;
            }

            Heading = string.Empty;
            Question = string.Empty;
            return Input ?? string.Empty;
        }
    }
}