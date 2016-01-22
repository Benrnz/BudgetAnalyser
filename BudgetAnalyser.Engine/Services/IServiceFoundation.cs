using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A UI Service is a service that marshalls and returns data to and encapsulates business functionality.
    ///     The main purpose being to store the business functionality inside a class with no ties to UI technology.
    ///     A service class should not contain anything UI technology specific, so it can be portable to other technologies.
    ///     For example WPF and Windows RT.
    ///     The methods of the class should be aligned with use cases of the UI.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces",
        Justification = "Useful to document the purpose of these service interfaces.")]
    public interface IServiceFoundation
    {
    }
}