using BudgetAnalyser.Engine;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Filtering
{
    public class RequestFilterChangeMessage : MessageBase
    {
        public RequestFilterChangeMessage(object sender)
        {
            Sender = sender;
        }

        public GlobalFilterCriteria Criteria { get; set; }
    }
}