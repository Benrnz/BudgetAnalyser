using System;
using System.Security;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.Storage
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public sealed class CredentialStore : ICredentialStore, IDisposable
    {
        private const string DefaultPassword = "Password123456789";

        // Track whether Dispose has been called. 
        private bool disposed;
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

        /// <summary>
        ///     Implement IDisposable.
        ///     Do not make this method virtual.
        ///     A derived class should not be able to override this method
        /// </summary>
        public void Dispose()
        {
            // Check to see if Dispose has already been called. 
            if (!this.disposed)
            {
                // Release unmanaged resources. If disposing is false, 
                // only the following code is executed. 
                this.passPhrase.Dispose();
                // Note that this is not thread safe. 
                // Another thread could start disposing the object 
                // after the managed resources are disposed, 
                // but before the disposed flag is set to true. 
                // If thread safety is necessary, it must be 
                // implemented by the client. 
            }

            this.disposed = true;

            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object 
            // from executing a second time. 
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="CredentialStore" /> class.
        ///     Use C# destructor syntax for finalization code.
        ///     This destructor will run only if the Dispose method
        ///     does not get called.
        ///     It gives your base class the opportunity to finalize.
        ///     Do not provide destructors in types derived from this class.
        /// </summary>
        ~CredentialStore()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability. 
            Dispose();
        }
    }
}