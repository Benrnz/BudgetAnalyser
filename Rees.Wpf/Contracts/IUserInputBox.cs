namespace Rees.Wpf.Contracts;

/// <summary>
///     Shows an input box to get one string of text from the user.
/// </summary>
public interface IUserInputBox
{
    /// <summary>
    ///     Show an input box with a heading and a question.
    /// </summary>
    /// <param name="heading">The window title</param>
    /// <param name="question">The main question text</param>
    /// <param name="defaultInput">An optional default value for the input box</param>
    /// <returns>User response to the question, or null if the user cancelled.  The user cannot return null if 'Ok' is clicked.</returns>
    string Show(string heading, string question, string defaultInput = "");
}
