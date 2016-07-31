namespace BudgetAnalyser.Engine.Persistence
{
    /// <summary>
    /// A generic interface to store and retrieve a credential.
    /// </summary>
    public interface ICredentialStore
    {
        /// <summary>
        /// Retrieves the pass key.
        /// </summary>
        object RetrievePasskey();

        /// <summary>
        /// Sets the pass key.
        /// </summary>
        void SetPasskey(object passkey);
    }
}
