using BudgetAnalyser.Engine;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser;

/// <summary>
///     Controllers required by the <see cref="ShellController" /> and most other <see cref="ControllerBase" /> controllers grouped together for convenience.
///     This follows an Ambient Context pattern. Not using Thread Local Storage for ease of testing.
/// </summary>
public class UiContext(UserPrompts userPrompts, IMessenger messenger, ILogger logger) : IUiContext
{
    private readonly Dictionary<Type, Lazy<ControllerBase>> controllerDic = new();
    private IReadOnlySet<ControllerBase> doNotUseControllers = new HashSet<ControllerBase>();

    public IReadOnlySet<ControllerBase> Controllers
    {
        get
        {
            if (this.doNotUseControllers.Any())
            {
                return this.doNotUseControllers;
            }

            return this.doNotUseControllers = this.controllerDic.Values.Select(c => c.Value).ToHashSet();
        }
    }

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
        foreach (var kvp in controllers)
        {
            this.controllerDic.Add(kvp.Key, kvp.Value);
        }
    }

    public ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));
    public IMessenger Messenger { get; } = messenger ?? throw new ArgumentNullException(nameof(messenger));
    public UserPrompts UserPrompts { get; } = userPrompts ?? throw new ArgumentNullException(nameof(userPrompts));
}
