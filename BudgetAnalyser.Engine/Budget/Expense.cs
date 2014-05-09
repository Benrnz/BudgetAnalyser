using System;
using System.Text;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    public class Expense : BudgetItem, IModelValidate
    {
        public bool Validate([NotNull] StringBuilder validationMessages)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException("validationMessages");
            }

            bool retval = Bucket.Validate(validationMessages);

            if (retval && Amount <= 0)
            {
                validationMessages.AppendFormat("Expense {0} with amount {1:C} is invalid, amount can not be zero or less.", Bucket.Description, Amount);
                retval = false;
            }

            if (retval && !(Bucket is ExpenseBudgetBucket))
            {
                validationMessages.AppendFormat("Expense {0} with amount {1:C} is invalid, you must allocate an expense bucket.", Bucket.Description, Amount);
                retval = false;
            }

            if (string.IsNullOrWhiteSpace(Bucket.Description))
            {
                validationMessages.AppendFormat("Expense with Amount {0:C} is invalid, Description can not be blank.", Amount);
                retval = false;
            }

            return retval;
        }
    }
}