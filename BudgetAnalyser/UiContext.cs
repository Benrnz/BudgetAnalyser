using BudgetAnalyser.Engine;
using Rees.Wpf;

namespace BudgetAnalyser;

/// <summary>
///     Controllers required by the <see cref="ShellController" /> and most other <see cref="ControllerBase" /> controllers grouped together for convenience.
///     This follows an Ambient Context pattern. Not using Thread Local Storage for ease of testing.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public class UiContext : IUiContext
{
    private readonly Dictionary<Type, Lazy<ControllerBase>> controllerDic = new();

    // TODO Ideally would like to remove controllers from the ambient context.
    public IReadOnlySet<ControllerBase> Controllers
    {
        get
        {
            if (field.Any())
            {
                return field;
            }

            return field = this.controllerDic.Values.Select(c => c.Value).ToHashSet();
        }
    } = new HashSet<ControllerBase>();

    public T Controller<T>() where T : ControllerBase
    {
        if (this.controllerDic.TryGetValue(typeof(T), out var lazy))
        {
            return (T)lazy.Value;
        }

        throw new ArgumentException($"Controller of type {typeof(T).Name} not found.");
    }

    public void Initialise(IDictionary<Type, Lazy<ControllerBase>> controllers)
    {
        // Don't call initialise on each controller here, that is the job of the Shell Controller when it is ready to.
        foreach (var kvp in controllers)
        {
            this.controllerDic.Add(kvp.Key, kvp.Value);
        }
    }
}
