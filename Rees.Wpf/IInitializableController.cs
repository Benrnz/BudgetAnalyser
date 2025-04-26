namespace Rees.Wpf;

/// <summary>
///     A utility interface to mark a controller as requiring some initialization before use. This is useful when a controller needs to do some preparation work before the first usage,
///     and it's inappropriate to put the initialization in the constructor.
/// </summary>
public interface IInitializableController
{
    /// <summary>
    ///     Initialise the controller to prepare it for use
    /// </summary>
    void Initialize();
}
