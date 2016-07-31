using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     An interface to allow the Application Database master service to communicate with subordinate database dependant
    ///     services. Only <see cref="IServiceFoundation" /> implementations should implement this.
    /// </summary>
    public interface ISupportsModelPersistence
    {
        /// <summary>
        ///     Gets the type of the data the implementation deals with.
        /// </summary>
        ApplicationDataType DataType { get; }

        /// <summary>
        ///     Gets the initialisation sequence number. Set this to a low number for important data that needs to be loaded first.
        ///     Defaults to 50.
        /// </summary>
        int LoadSequence { get; }

        /// <summary>
        ///     Closes the currently loaded file.  No warnings will be raised if there is unsaved data.
        /// </summary>
        void Close();

        /// <summary>
        ///     Create a new file specific for that service's data.
        /// </summary>
        Task CreateAsync([NotNull] ApplicationDatabase applicationDatabase);

        /// <summary>
        ///     Loads a data source with the provided database reference data asynchronously.
        /// </summary>
        Task LoadAsync([NotNull] ApplicationDatabase applicationDatabase);

        /// <summary>
        ///     Saves the application database asynchronously. This may be called using a background worker thread.
        /// </summary>
        Task SaveAsync([NotNull] ApplicationDatabase applicationDatabase);

        /// <summary>
        ///     Called before Save is called. This will be called on the UI Thread.
        ///     Objects can optionally add some context data that will be passed to the <see cref="SaveAsync" /> method call.
        ///     This can be used to finalise any edits or prompt the user for closing data, ie, a "what-did-you-change" comment;
        ///     this can't be done during save as it may not be called using the UI Thread.
        /// </summary>
        void SavePreview();

        /// <summary>
        ///     Validates the model owned by the service.
        /// </summary>
        bool ValidateModel([NotNull] StringBuilder messages);
    }
}