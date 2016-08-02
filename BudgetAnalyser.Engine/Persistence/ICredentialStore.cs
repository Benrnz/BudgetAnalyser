namespace BudgetAnalyser.Engine.Persistence
{
    /// <summary>
    ///     A generic interface to store and retrieve a credential.
    /// </summary>
    public interface ICredentialStore
    {
        /// <summary>
        ///     Retrieves the pass key.
        /// </summary>
        object RetrievePasskey();

        /// <summary>
        ///     Retrieves the pass key.
        /// </summary>
        /// <returns>A credential object or null if no credentials have been provided by the user.</returns>
        void SetPasskey(object passkey);
    }
}