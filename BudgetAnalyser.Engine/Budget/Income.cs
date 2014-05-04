using System;
using System.Text;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    public class Income : BudgetItem, IModelValidate
    {
        public bool Validate([NotNull] StringBuilder validationMessages)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException("validationMessages");
            }

            bool retval = Bucket.Validate(validationMessages);

            if (retval && Bucket.GetType() != typeof(IncomeBudgetBucket))
            {
                validationMessages.AppendFormat("Income {0} with amount {1:C} is invalid, you must allocate an income bucket.", Bucket.Description, Amount);
                retval = false;
            }

            if (retval && Amount <= 0)
            {
                validationMessages.AppendFormat("Income {0} with Amount {1:C} is invalid, it can not be zero or less.", Bucket.Description, Amount);
                retval = false;
            }

            if (string.IsNullOrWhiteSpace(Bucket.Description))
            {
                validationMessages.AppendFormat("Income with Amount {0:C} is invalid, Description can not be blank.", Amount);
                retval = false;
            }

            return retval;
        }
    }
}