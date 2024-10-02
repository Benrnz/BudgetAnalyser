using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Encryption
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal sealed class SecureStringCredentialStore : ICredentialStore, IDisposable
    {
        // Track whether Dispose has been called. 
        private bool disposed;
        private SecureString passPhrase = new SecureString();

        /// <summary>
        ///     Retrieves the pass key.
        /// </summary>
        /// <returns>A credential object or null if no credentials have been provided by the user.</returns>
        public object RetrievePasskey()
        {
            if (this.passPhrase == null || this.passPhrase.Length == 0) return null;
            return this.passPhrase;
        }

        /// <summary>
        ///     Sets the pass key.
        /// </summary>
        public void SetPasskey(object passkey)
        {
            this.passPhrase?.Dispose();
            this.passPhrase = (SecureString) passkey;
        }

        /// <summary>
        ///     Implement IDisposable.
        ///     Do not make this method virtual.
        ///     A derived class should not be able to override this method
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "passPhrase", Justification = "False positive, it is being disposed (C#6 syntax issue)")]
        public void Dispose()
        {
            // Check to see if Dispose has already been called. 
            if (!this.disposed)
            {
                // Release unmanaged resources. If disposing is false, 
                // only the following code is executed. 
                this.passPhrase?.Dispose();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SecureStrings")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SecureStringCredentialStore")]
        public bool AreEqual(object compareTo)
        {
            if (this.passPhrase == null || compareTo == null)
            {
                return false;
            }

            var key1 = this.passPhrase;
            var key2 = compareTo as SecureString;
            if (key2 == null)
            {
                throw new NotSupportedException($"{nameof(SecureStringCredentialStore)} only supports use of SecureStrings.");
            }

            if (key1.Length != key2.Length)
            {
                return false;
            }

            var bstr1 = IntPtr.Zero;
            var bstr2 = IntPtr.Zero;

            RuntimeHelpers.PrepareConstrainedRegions();

            try
            {
                bstr1 = Marshal.SecureStringToBSTR(key1);
                bstr2 = Marshal.SecureStringToBSTR(key2);

                unsafe
                {
                    for (char* ptr1 = (char*) bstr1.ToPointer(), ptr2 = (char*) bstr2.ToPointer();
                        *ptr1 != 0 && *ptr2 != 0;
                        ++ptr1, ++ptr2)
                    {
                        if (*ptr1 != *ptr2)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            finally
            {
                if (bstr1 != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr1);
                }

                if (bstr2 != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr2);
                }
            }
        }

        internal static string SecureStringToString(SecureString value)
        {
            var valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="SecureStringCredentialStore" /> class.
        ///     Use C# destructor syntax for finalization code.
        ///     This destructor will run only if the Dispose method
        ///     does not get called.
        ///     It gives your base class the opportunity to finalize.
        ///     Do not provide destructors in types derived from this class.
        /// </summary>
        ~SecureStringCredentialStore()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability. 
            Dispose();
        }
    }
}