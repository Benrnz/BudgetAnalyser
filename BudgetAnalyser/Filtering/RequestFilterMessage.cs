using BudgetAnalyser.Engine;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Filtering
{
    public class RequestFilterMessage : MessageBase
    {
        public RequestFilterMessage(object sender)
        {
            Sender = sender;
        }

        public GlobalFilterCriteria Criteria { get; set; }
    }
}