using System.Text;

namespace BudgetAnalyser.Engine.Budget
{
    public class Income : BudgetItem, IModelValidate
    {
        private decimal amount;

        public override decimal Amount
        {
            get { return this.amount; }
            set
            {
                if (value == this.amount)
                {
                    return;
                }
                this.amount = value;
                OnPropertyChanged();
            }
        }

        public bool Validate(StringBuilder validationMessages)
        {
            bool retval = this.Bucket.Validate(validationMessages);

            if (retval && this.Bucket.GetType() != typeof(IncomeBudgetBucket))
            {
                validationMessages.AppendFormat("Income {0} with amount {1:C} is invalid, you must allocate an income bucket.", this.Bucket.Description, this.Amount);
                retval = false;
            }

            if (retval && this.Amount <= 0)
            {
                validationMessages.AppendFormat("Income {0} with Amount {1:C} is invalid, it can not be zero or less.", this.Bucket.Description, this.Amount);
                retval = false;
            }

            if (string.IsNullOrWhiteSpace(this.Bucket.Description))
            {
                validationMessages.AppendFormat("Income with Amount {0:C} is invalid, Description can not be blank.", this.Amount);
                retval = false;
            }

            return retval;
        }
    }
}