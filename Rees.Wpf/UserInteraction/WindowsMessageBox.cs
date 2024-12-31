using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;

namespace Rees.Wpf.UserInteraction
{
    /// <summary>
    ///     A Wpf implementation of a simple Message box to show to the user. It only can have an Ok button.
    /// </summary>
    public class WindowsMessageBox : MessageBoxBase
    {
        /// <summary>
        ///     Show a message with a message and heading.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="headingCaption">The optional heading.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed",
                         Justification = "Optional parameters are prefered unless theres a reason not to.")]
        public override void Show(string message, string headingCaption = "")
        {
            string heading;
            var unowned = false;
            try
            {
                heading = string.IsNullOrWhiteSpace(headingCaption)
                    ? Application.Current.MainWindow.Title ?? string.Empty
                    : headingCaption;
            }
            catch (InvalidOperationException)
            {
                // This will occur if another thread accesses Application.Current.MainWindow other than the main thread.
                heading = string.Empty;
                unowned = true;
            }

            var content = RationaliseMessage(message);


            if (unowned)
            {
                MessageBox.Show(content, heading);
                return;
            }

            try
            {
                if (Application.Current.MainWindow is not null)
                {
                    MessageBox.Show(Application.Current.MainWindow, content, heading);
                }
                else
                {
                    MessageBox.Show(content);
                }
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(content, heading);
            }
        }

        /// <summary>
        ///     Show a message in regard to an exception that occured.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="message">The message.</param>
        public override void Show(Exception ex, string message)
        {
            if (ex is null)
            {
                Show(message);
                return;
            }

            var exText = ex.ToString();
            if (exText.Length > 400)
            {
                exText = exText.Substring(0, 400);
            }

            Show(message + "\n\n" + exText);
        }

        /// <summary>
        ///     Show a messgae using a string format.
        /// </summary>
        /// <param name="format">The message.</param>
        /// <param name="argument1">The first argument.</param>
        /// <param name="args">Other arguments.</param>
        public override void Show(string format, object argument1, params object[] args)
        {
            Show(string.Format(CultureInfo.CurrentCulture, format, PrependElement(argument1, args)));
        }

        /// <summary>
        ///     Show a message using a string format and a heading.
        /// </summary>
        /// <param name="headingCaption">The optional heading.</param>
        /// <param name="format">The message.</param>
        /// <param name="argument1">The first argument.</param>
        /// <param name="args">Other arguments.</param>
        public override void Show(string headingCaption, string format, object argument1, params object[] args)
        {
            Show(string.Format(CultureInfo.CurrentCulture, format, PrependElement(argument1, args)), headingCaption);
        }

        /// <summary>
        ///     Show a message in regard to an exception that occured.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="format">The message.</param>
        /// <param name="argument1">The first argument.</param>
        /// <param name="args">Other arguments.</param>
        public override void Show(Exception ex, string format, object argument1, params object[] args)
        {
            Show(ex, string.Format(CultureInfo.CurrentCulture, format, PrependElement(argument1, args)));
        }

        private static string RationaliseMessage(string message)
        {
            if (message.Length > 1024)
            {
                message = message.Substring(0, 1024) + "\n---TRUNCATED---";
            }

            return message;
        }
    }
}
