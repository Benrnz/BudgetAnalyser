namespace Rees.Wpf.Contracts
{
    /// <summary>
    ///     A class used to encapsulate any model that needs to be persistented.  It gives the consumer a opportunity to store
    ///     any metadata about the persisted model as well as top level versioning.
    /// </summary>
    public interface IPersistent
    {
        /// <summary>
        ///     Gets the sequence number for this implementation.
        ///     This is used to load more crucial higher priority persistent data first, if any.
        /// </summary>
        int LoadSequence { get; }
    }
}