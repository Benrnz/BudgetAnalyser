using System.Globalization;
using System.Text;

namespace BudgetAnalyser.Engine.Budget;

/// <summary>
///     An income entry in the budget. This represents a user created amount and bucket for an income in a budget.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Budget.BudgetItem" />
/// <seealso cref="BudgetAnalyser.Engine.IModelValidate" />
public class Income : BudgetItem, IModelValidate
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

        if (Bucket is null)
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Bucket has not been set on this Income. {0:C}", Amount);
            return false;
        }

        var result = Bucket.Validate(validationMessages);

        if (result && Bucket.GetType() != typeof(IncomeBudgetBucket))
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Income {0} with amount {1:C} is invalid, you must allocate an income bucket.", Bucket.Description, Amount);
            result = false;
        }

        if (result && Amount < 0)
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Income {0} with Amount {1:C} is invalid, it can't be less than zero.", Bucket.Description, Amount);
            result = false;
        }

        if (string.IsNullOrWhiteSpace(Bucket.Description))
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Income with Amount {0:C} is invalid, Description can not be blank.", Amount);
            result = false;
        }

        return result;
    }
}
