using System;
using System.Globalization;
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
                throw new ArgumentNullException(nameof(validationMessages));
            }

            bool retval = Bucket.Validate(validationMessages);

            if (retval && Amount < 0)
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Expense {0} with amount {1:C} is invalid, amount can't be less than zero.", Bucket.Description, Amount);
                retval = false;
            }

            if (retval && Amount == 0 && Bucket.Active)
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Expense {0} with amount {1:C} is invalid, amount can't be zero.", Bucket.Description, Amount);
                retval = false;
            }

            if (retval && !(Bucket is ExpenseBucket))
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Expense {0} with amount {1:C} is invalid, you must allocate an expense bucket.", Bucket.Description, Amount);
                retval = false;
            }

            if (string.IsNullOrWhiteSpace(Bucket.Description))
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Expense with Amount {0:C} is invalid, Description can not be blank.", Amount);
                retval = false;
            }

            return retval;
        }
    }
}