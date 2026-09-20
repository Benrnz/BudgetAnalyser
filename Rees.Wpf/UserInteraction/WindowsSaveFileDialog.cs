using Microsoft.Win32;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.UserInteraction;

/// <summary>
///     A Wpf implementation to show the Save File Dialog so the user can choose a file.
/// </summary>
public class WindowsSaveFileDialog : FileDialogBase<SaveFileDialog>, IUserPromptSaveFile
{
}
