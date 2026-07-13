using BudgetAnalyser.Engine;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser;

[AutoRegisterWithIoC(SingleInstance = true)]
public class UserPrompts
{
    public UserPrompts(
        IUserMessageBox messageBox,
        Func<IUserPromptOpenFile> openFileFactory,
        Func<IUserPromptSaveFile> saveFileFactory,
        IUserQuestionBoxYesNo yesNoBox,
        IUserInputBox inputBox)
    {
        OpenFileFactory = openFileFactory ?? throw new ArgumentNullException(nameof(openFileFactory));
        SaveFileFactory = saveFileFactory ?? throw new ArgumentNullException(nameof(saveFileFactory));
        MessageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));
        YesNoBox = yesNoBox ?? throw new ArgumentNullException(nameof(yesNoBox));
        InputBox = inputBox ?? throw new ArgumentNullException(nameof(inputBox));
    }

    public IUserInputBox InputBox { get; private set; }
    public IUserMessageBox MessageBox { get; private set; }
    public Func<IUserPromptOpenFile> OpenFileFactory { get; private set; }
    public Func<IUserPromptSaveFile> SaveFileFactory { get; private set; }
    public IUserQuestionBoxYesNo YesNoBox { get; private set; }
}
