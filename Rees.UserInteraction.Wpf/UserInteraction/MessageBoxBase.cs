using System;
using System.Diagnostics.CodeAnalysis;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.UserInteraction
{
    /// <summary>
    /// An abstract base class with some common functionality for derived Wpf message box implementations.
    /// </summary>
    public abstract class MessageBoxBase : IUserMessageBox
    {
        /// <summary>
        /// Show a message with a message and heading.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="headingCaption">The optional heading.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed",
            Justification = "Optional parameters are prefered unless theres a reason not to.")]
        public abstract void Show(string message, string headingCaption = "");

        /// <summary>
        /// Show a message using a string format and a heading.
        /// </summary>
        /// <param name="headingCaption">The optional heading.</param>
        /// <param name="format">The message.</param>
        /// <param name="argument1">The first argument.</param>
        /// <param name="args">Other arguments.</param>
        public abstract void Show(string headingCaption, string format, object argument1, params object[] args);

        /// <summary>
        /// Show a messgae using a string format.
        /// </summary>
        /// <param name="format">The message.</param>
        /// <param name="argument1">The first argument.</param>
        /// <param name="args">Other arguments.</param>
        public abstract void Show(string format, object argument1, params object[] args);

        /// <summary>
        /// Show a message in regard to an exception that occured.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="message">The message.</param>
        public abstract void Show(Exception ex, string message);

        /// <summary>
        /// Show a message in regard to an exception that occured.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message.</param>
        /// <param name="argument1">The first argument.</param>
        /// <param name="args">Other arguments.</param>
        public abstract void Show(Exception ex, string format, object argument1, params object[] args);

        /// <summary>
        /// Inserts a new element at the beginning of an array. The array will be copied.
        /// </summary>
        /// <param name="newElement">The new element.</param>
        /// <param name="existingArray">The existing array.</param>
        /// <returns>A new array with the element inserted.</returns>
        /// <exception cref="System.ArgumentNullException">existingArray cannot be null</exception>
        protected static object[] PrependElement(object newElement, object[] existingArray)
        {
            if (existingArray == null)
            {
                throw new ArgumentNullException("existingArray");
            }

            object[] stringArgs;
            if (existingArray.Length > 0)
            {
                stringArgs = new object[existingArray.Length + 1];
                Array.Copy(existingArray, 0, stringArgs, 1, existingArray.Length);
                stringArgs[0] = newElement;
            }
            else
            {
                stringArgs = new[] {newElement};
            }

            return stringArgs;
        }
    }
}