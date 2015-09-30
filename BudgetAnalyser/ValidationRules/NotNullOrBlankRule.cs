using System;
using System.Globalization;
using System.Windows.Controls;

namespace BudgetAnalyser.ValidationRules
{
    /// <summary>
    /// Ensures the value is not null or string.Empty.
    /// </summary>
    public class NotNullOrBlankRule : ValidationRule
    {
        /// <summary>
        /// When overridden in a derived class, performs validation checks on a value.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Controls.ValidationResult"/> object.
        /// </returns>
        /// <param name="value">The value from the binding target to check.</param><param name="cultureInfo">The culture to use in this rule.</param>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            if (str != null)
            {
                return string.IsNullOrWhiteSpace(str) ? new ValidationResult(false, "Please enter a value (other than spaces).") : new ValidationResult(true, null);
            }

            return new ValidationResult(false, "Please enter a value.");
        }
    }
}
