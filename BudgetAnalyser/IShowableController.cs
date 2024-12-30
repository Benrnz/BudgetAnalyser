namespace BudgetAnalyser
{
    /// <summary>
    ///     An interface that uses a data bound Shown property to show or hide the controller
    ///     which in turn controls the visibility of the controller's view.
    /// </summary>
    public interface IShowableController
    {
        bool Shown { get; set; }
    }
}