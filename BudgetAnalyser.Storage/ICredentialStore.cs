using System.Security;

namespace BudgetAnalyser.Storage
{
    public interface ICredentialStore
    {
        SecureString RetrievePassword();

        void SetPassword(SecureString password);
    }
}
