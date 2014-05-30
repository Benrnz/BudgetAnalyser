using System;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Statement
{
    public class NavigateToTransactionMessage : MessageBase
    {
        private readonly CancellationTokenSource cancellationSource;
        private bool success;

        public NavigateToTransactionMessage(Guid transactionId)
        {
            TransactionId = transactionId;
            this.cancellationSource = new CancellationTokenSource();
            CancellationToken cancellationToken = this.cancellationSource.Token;
            WhenReadyToNavigate = new Task(() => { }, cancellationToken);
        }

        public bool Success
        {
            get { return this.success; }
        }

        public Guid TransactionId { get; private set; }
        public Task WhenReadyToNavigate { get; private set; }

        public void SetSearchAsFailed()
        {
            this.success = false;
            this.cancellationSource.Cancel();
        }

        public void SetSearchAsSuccessful()
        {
            this.success = true;
            WhenReadyToNavigate.Start();
        }
    }
}