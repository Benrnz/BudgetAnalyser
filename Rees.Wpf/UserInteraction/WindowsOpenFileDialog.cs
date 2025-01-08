using System.Windows;
using Microsoft.Win32;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.UserInteraction;

/// <summary>
///     A Wpf implementation to show the Windows File Open Dialog so the user can choose a file.
/// </summary>
public class WindowsOpenFileDialog : IUserPromptOpenFile
{
    /// <summary>
    ///     The optional extension to include as an alternative file extension to filter to.
    /// </summary>
    public bool? AddExtension { get; set; }

    /// <summary>
    ///     Indicates if an optional check that can be performed to check if the user selected file exists.
    /// </summary>
    public bool? CheckFileExists { get; set; }

    /// <summary>
    ///     Indicates if an optional check that can be performed to check if the user selected folder exists.
    /// </summary>
    public bool? CheckPathExists { get; set; }

    /// <summary>
    ///     The optional extension to include to filter file selection to.
    /// </summary>
    public string DefaultExt { get; set; } = string.Empty;

    /// <summary>
    ///     The resulting file the user has selected.
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the filter string that determines what types of files are displayed.
    /// </summary>
    public string Filter { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the index of the filter currently selected in a file dialog.
    /// </summary>
    public int? FilterIndex { get; set; }

    /// <summary>
    ///     The title for the dialog.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Show the file selection dialog box.
    /// </summary>
    /// <returns>
    ///     true to indicate successful selection, false to idicate a problem with file selection, null to indicate
    ///     cancellation.
    /// </returns>
    public bool? ShowDialog()
    {
        var dialog = new OpenFileDialog();
        if (AddExtension is not null)
        {
            dialog.AddExtension = AddExtension.Value;
        }

        if (CheckFileExists is not null)
        {
            dialog.CheckFileExists = CheckFileExists.Value;
        }

        if (CheckPathExists is not null)
        {
            dialog.CheckPathExists = CheckPathExists.Value;
        }

        dialog.DefaultExt = DefaultExt;
        dialog.Title = Title;
        dialog.Filter = Filter;
        dialog.FilterIndex = FilterIndex ?? 0;

        bool? result;
        try
        {
            var owner = Application.Current.MainWindow;
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
