using Microsoft.Win32;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.UserInteraction;

/// <summary>
///     A Wpf implementation to show the Windows File Open Dialog so the user can choose a file.
/// </summary>
public class WindowsOpenFileDialog : FileDialogBase<OpenFileDialog>, IUserPromptOpenFile
{
    /// <summary>
    ///     Indicates if an optional check that can be performed to check if the user selected file exists.
    /// </summary>
    public bool? CheckFileExists { get; set; }

    /// <inheritdoc />
    protected override void ConfigureDialog(OpenFileDialog dialog)
    {
        if (CheckFileExists is not null)
        {
            dialog.CheckFileExists = CheckFileExists.Value;
        }
    }
}
