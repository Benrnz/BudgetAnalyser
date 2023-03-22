namespace Rees.Wpf.Contracts
{
    /// <summary>
    ///     Represents a file open or choose file dialog.
    /// </summary>
    public interface IUserPromptOpenFile
    {
        /// <summary>
        ///     The optional extension to include as an alternative file extension to filter to.
        /// </summary>
        bool? AddExtension { get; set; }

        /// <summary>
        ///     Indicates if an optional check that can be performed to check if the user selected file exists.
        /// </summary>
        bool? CheckFileExists { get; set; }

        /// <summary>
        ///     Indicates if an optional check that can be performed to check if the user selected folder exists.
        /// </summary>
        bool? CheckPathExists { get; set; }

        /// <summary>
        ///     The optional extension to include to filter file selection to.
        /// </summary>
        string DefaultExt { get; set; }

        /// <summary>
        ///     The resulting file the user has selected.
        /// </summary>
        string FileName { get; }

        /// <summary>
        ///     Gets or sets the filter string that determines what types of files are displayed.
        /// </summary>
        string Filter { get; set; }

        /// <summary>
        ///     Gets or sets the index of the filter currently selected in a file dialog.
        /// </summary>
        int? FilterIndex { get; set; }

        /// <summary>
        ///     The title for the dialog.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        ///     Show the file selection dialog box.
        /// </summary>
        /// <returns>
        ///     true to indicate successful selection, false to idicate a problem with file selection, null to indicate
        ///     cancellation.
        /// </returns>
        bool? ShowDialog();
    }
}