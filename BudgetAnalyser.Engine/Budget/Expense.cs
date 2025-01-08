using System.Globalization;
using System.Text;

namespace BudgetAnalyser.Engine.Budget;

/// <summary>
///     An expense line in a budget. Created by the user to represent an expense line in a budget with an amount and bucket
///     classification.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Budget.BudgetItem" />
/// <seealso cref="BudgetAnalyser.Engine.IModelValidate" />
public class Expense : BudgetItem, IModelValidate
{
    /// <summary>
    ///     Validate the instance and populate any warnings and errors into the <paramref name="validationMessages" /> string
    ///     builder.
    /// </summary>
    /// <param name="validationMessages">A non-null string builder that will be appended to for any messages.</param>
    /// <returns>
    ///     If the instance is in an invalid state it will return false, otherwise it returns true.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool Validate(StringBuilder validationMessages)
    {
        if (validationMessages is null)
        {
            throw new ArgumentNullException(nameof(validationMessages));
        }

        var result = Bucket.Validate(validationMessages);

        if (result && Amount < 0)
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Expense {0} with amount {1:C} is invalid, amount can't be less than zero.", Bucket.Description, Amount);
            result = false;
        }

        if (result && Amount == 0 && Bucket.Active)
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Expense {0} with amount {1:C} is invalid, amount can't be zero.", Bucket.Description, Amount);
            result = false;
        }

        if (result && !(Bucket is ExpenseBucket))
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Expense {0} with amount {1:C} is invalid, you must allocate an expense bucket.", Bucket.Description, Amount);
            result = false;
        }

        if (string.IsNullOrWhiteSpace(Bucket.Description))
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Expense with Amount {0:C} is invalid, Description can not be blank.", Amount);
            result = false;
        }

        return result;
    }
}
