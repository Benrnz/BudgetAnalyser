using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.ApplicationState
{
    /// <summary>
    /// A <see cref="MessageBase"/> message object that signifies the application state has finished loading.  
    /// This is useful to know so that components can begin processing knowing that all other components have loaded their required start up data.
    /// </summary>
    public class ApplicationStateLoadFinishedMessage : MessageBase
    {
    }
}