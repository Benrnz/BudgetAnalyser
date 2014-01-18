using System.Text;

namespace BudgetAnalyser.Engine.Budget
{
    public class Expense : BudgetItem, IModelValidate
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
                this.OnPropertyChanged();
            }
        }

        public bool Validate(StringBuilder validationMessages)
        {
            bool retval = this.Bucket.Validate(validationMessages);

            if (retval && this.Amount <= 0)
            {
                validationMessages.AppendFormat("Expense {0} with amount {1:C} is invalid, amount can not be zero or less.", this.Bucket.Description, this.Amount);
                retval = false;
            }

            if (retval && !(this.Bucket is ExpenseBudgetBucket))
            {
                validationMessages.AppendFormat("Expense {0} with amount {1:C} is invalid, you must allocate an expense bucket.", this.Bucket.Description, this.Amount);
                retval = false;
            }

            return retval;
        }
    }
}