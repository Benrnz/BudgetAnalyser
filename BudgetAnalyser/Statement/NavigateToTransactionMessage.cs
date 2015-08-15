using System;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Statement
{
    public sealed class NavigateToTransactionMessage : MessageBase, IDisposable
    {
        private readonly CancellationTokenSource cancellationSource;
        private bool disposed;

        public NavigateToTransactionMessage(Guid transactionId)
        {
            TransactionId = transactionId;
            this.cancellationSource = new CancellationTokenSource();
            CancellationToken cancellationToken = this.cancellationSource.Token;
            WhenReadyToNavigate = new Task(() => { }, cancellationToken);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="NavigateToTransactionMessage" /> class.
        ///     Use C# destructor syntax for finalization code.
        ///     This destructor will run only if the Dispose method
        ///     does not get called.
        ///     It gives your base class the opportunity to finalize.
        ///     Do not provide destructors in types derived from this class.
        /// </summary>
        ~NavigateToTransactionMessage()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability. 
            Dispose(false);
        }

        public bool Success { get; private set; }
        public Guid TransactionId { get; private set; }
        public Task WhenReadyToNavigate { get; }

        /// <summary>
        ///     Implement IDisposable.
        ///     Do not make this method virtual.
        ///     A derived class should not be able to override this method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object 
            // from executing a second time. 
            GC.SuppressFinalize(this);
        }

        public void SetSearchAsFailed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("NavigateToTransactionMessage");
            }

            Success = false;
            this.cancellationSource.Cancel();
        }

        public void SetSearchAsSuccessful()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("NavigateToTransactionMessage");
            }

            Success = true;
            WhenReadyToNavigate.Start();
        }

        /// <summary>
        ///     Dispose(bool disposing) executes in two distinct scenarios.
        ///     If disposing equals true, the method has been called directly
        ///     or indirectly by a user's code. Managed and unmanaged resources
        ///     can be disposed.
        ///     If disposing equals false, the method has been called by the
        ///     runtime from inside the finalizer and you should not reference
        ///     other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged
        ///     resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources. 
                    WhenReadyToNavigate.Dispose();
                    this.cancellationSource.Dispose();
                }
            }

            this.disposed = true;
        }
    }
}