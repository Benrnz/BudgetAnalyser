﻿using System.Text;

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
                OnPropertyChanged();
            }
        }

        public bool Validate(StringBuilder validationMessages)
        {
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

            return retval;
        }
    }
}