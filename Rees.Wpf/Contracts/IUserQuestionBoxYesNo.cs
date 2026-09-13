namespace Rees.Wpf.Contracts;

/// <summary>
///     Represents a question to the user towhich the user can only respond with Yes, No, or Cancel.
/// </summary>
public interface IUserQuestionBoxYesNo
{
    /// <summary>
    ///     Show the question box with a caption and heading.
    /// </summary>
    /// <param name="question">The yes/no question to show on the dialog.</param>
    /// <param name="heading">The optional heading for the dialog.</param>
    /// <returns>true for yes, false for no, and null for cancellation.</returns>
    bool? Show(string question, string heading = "");

    /// <summary>
    ///     Show the question box with a caption and heading.
    /// </summary>
    /// <param name="heading">The optional heading for the dialog.</param>
    /// <param name="questionFormat">The yes/no question to show on the dialog.</param>
    /// <param name="argument1">The first argument for the string format.</param>
    /// <param name="args">All other arguments for the string format.</param>
    /// <returns>true for yes, false for no, and null for cancellation.</returns>
    bool? Show(string heading, string questionFormat, object argument1, params object[] args);
}
