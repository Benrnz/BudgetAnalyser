using Rees.Wpf;

namespace BudgetAnalyser;

/// <summary>
///     The idea to prevent ambient UI context objects that are required by all UI Controllers and ViewModels from  appearing in every constructor.
/// </summary>
public interface IUiContext
{
    // TODO Ideally would like to remove controllers from the ambient context.
    IReadOnlySet<ControllerBase> Controllers { get; }

    T Controller<T>() where T : ControllerBase;
    void Initialise(IDictionary<Type, Lazy<ControllerBase>> controllers);
}
