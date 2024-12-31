using System;
using System.Diagnostics.CodeAnalysis;

namespace Rees.Wpf.Contracts
{
    /// <summary>
    ///     Represents a means to show a message to the user.
    /// </summary>
    public interface IUserMessageBox
    {
        /// <summary>
        ///     Show a message with a message and heading.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="headingCaption">The optional heading.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed",
                         Justification = "Not possible to use all optional parameters with string-format params")]
        void Show(string message, string headingCaption = "");

        /// <summary>
        ///     Show a messgae using a string format.
        /// </summary>
        /// <param name="format">The message.</param>
        /// <param name="argument1">The first argument.</param>
        /// <param name="args">Other arguments.</param>
        void Show(string format, object argument1, params object[] args);

        /// <summary>
        ///     Show a message using a string format and a heading.
        /// </summary>
        /// <param name="headingCaption">The optional heading.</param>
        /// <param name="format">The message.</param>
        /// <param name="argument1">The first argument.</param>
        /// <param name="args">Other arguments.</param>
        void Show(string headingCaption, string format, object argument1, params object[] args);

        /// <summary>
        ///     Show a message in regard to an exception that occured.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="message">The message.</param>
        void Show(Exception ex, string message);

        /// <summary>
        ///     Show a message in regard to an exception that occured.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message.</param>
        /// <param name="argument1">The first argument.</param>
        /// <param name="args">Other arguments.</param>
        void Show(Exception ex, string format, object argument1, params object[] args);
    }
}
