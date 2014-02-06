namespace BudgetAnalyser
{
    /// <summary>
    ///     An inteface describing a background job that is used to show information about the job
    ///     and status while its running.
    /// </summary>
    public interface IBackgroundProcessingJobMetadata
    {
        /// <summary>
        ///     The description of the job
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Determines if the main menu is available while this job is running.
        /// </summary>
        bool MenuAvailable { get; }

        /// <summary>
        ///     Mark the job as finished. This is executed when the job is known to be finished.
        /// </summary>
        void Finish();

        /// <summary>
        ///     Start the job
        /// </summary>
        void StartNew(string description, bool menuAvailable);
    }
}