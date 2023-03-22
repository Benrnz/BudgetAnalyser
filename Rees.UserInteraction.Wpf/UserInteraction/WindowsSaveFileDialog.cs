using System;
using System.Windows;
using Microsoft.Win32;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.UserInteraction
{
    /// <summary>
    /// A Wpf implementation to show the Save File Dialog so the user can choose a file.
    /// </summary>
    public class WindowsSaveFileDialog : IUserPromptSaveFile
    {
        /// <summary>
        /// The optional extension to include as an alternative file extension to filter to.
        /// </summary>
        public bool? AddExtension { get; set; }

        /// <summary>
        /// Indicates if an optional check that can be performed to check if the user selected folder exists.
        /// </summary>
        public bool? CheckPathExists { get; set; }

        /// <summary>
        /// The optional extension to include to filter file selection to.
        /// </summary>
        public string DefaultExt { get; set; }

        /// <summary>
        /// The resulting file the user has selected.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets or sets the filter string that determines what types of files are displayed.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Gets or sets the index of the filter currently selected in a file dialog.
        /// </summary>
        public int? FilterIndex { get; set; }

        /// <summary>
        /// The title for the dialog.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Show the file selection dialog box.
        /// </summary>
        /// <returns>
        /// true to indicate successful selection, false to idicate a problem with file selection, null to indicate
        /// cancellation.
        /// </returns>
        public bool? ShowDialog()
        {
            var dialog = new SaveFileDialog();
            if (AddExtension != null)
            {
                dialog.AddExtension = AddExtension.Value;
            }

            if (CheckPathExists != null)
            {
                dialog.CheckPathExists = CheckPathExists.Value;
            }

            if (DefaultExt != null)
            {
                dialog.DefaultExt = DefaultExt;
            }

            if (Title != null)
            {
                dialog.Title = Title;
            }

            if (Filter != null)
            {
                dialog.Filter = Filter;
            }

            if (FilterIndex != null)
            {
                dialog.FilterIndex = FilterIndex.Value;
            }

            bool? result;
            try
            {
                Window owner = Application.Current.MainWindow;
                result = dialog.ShowDialog(owner);
            }
            catch (InvalidOperationException)
            {
                result = dialog.ShowDialog();
            }

            FileName = dialog.FileName;
            return result;
        }
    }
}