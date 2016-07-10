using System.Security;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.Storage
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class CredentialStore : ICredentialStore
    {
        // TODO Maybe use the absense of a password to detect that encryption is disabled?
        private const string DefaultPassword = "Password123456789";
        private SecureString passPhrase = new SecureString();

        public CredentialStore()
        {
            foreach (var c in DefaultPassword.ToCharArray())
            {
                this.passPhrase.AppendChar(c);
            }

        }

        public SecureString RetrievePassword()
        {
            return this.passPhrase;
        }

        public void SetPassword(SecureString password)
        {
            this.passPhrase = password;
        }
    }
}