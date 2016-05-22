using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    /// <summary>
    ///     A class used to store a criteria value when creating a new <see cref="MatchingRule" />.
    /// </summary>
    /// <typeparam name="T">
    ///     The type used to contain the value to match. Can be a class or a struct. A Null value indicates
    ///     there is no criteria present.
    /// </typeparam>
    public abstract class Criteria<T> : INotifyPropertyChanged
    {
        private bool doNotUseApplicable;
        private T doNotUseValue;

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Criteria{T}" /> will be applied as criteria.
        /// </summary>
        /// <value>
        ///     <c>true</c> if applicable; otherwise, <c>false</c> and will not be used in matching.
        /// </value>
        public bool Applicable
        {
            get { return this.doNotUseApplicable; }
            set
            {
                if (value == this.doNotUseApplicable)
                {
                    return;
                }
                this.doNotUseApplicable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the criteria value.
        /// </summary>
        public T Value
        {
            get { return this.doNotUseValue; }
            set
            {
                if (Equals(value, this.doNotUseValue))
                {
                    return;
                }
                this.doNotUseValue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Determines whether the value is equal to the <paramref name="operand2" /> but not niether can be blank or null.
        /// </summary>
        public abstract bool IsEqualButNotBlank(T operand2);

        /// <summary>
        ///     Called when a property is changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    ///     A string criteria value.
    /// </summary>
    /// <seealso cref="string" />
    public class StringCriteria : Criteria<string>
    {
        /// <summary>
        ///     Determines whether the value is equal to the <paramref name="operand2" /> but not niether can be blank or null.
        /// </summary>
        public override bool IsEqualButNotBlank(string operand2)
        {
            if (string.IsNullOrWhiteSpace(Value) || string.IsNullOrWhiteSpace(operand2))
            {
                return false;
            }

            var match = Value == operand2;
            return match;
        }
    }

    /// <summary>
    ///     A decimal criteria value.
    /// </summary>
    /// <seealso cref="decimal" />
    public class DecimalCriteria : Criteria<decimal?>
    {
        /// <summary>
        ///     Determines whether the value is equal to the <paramref name="operand2" /> but not niether can be blank or null.
        /// </summary>
        public override bool IsEqualButNotBlank(decimal? operand2)
        {
            if (Value == default(decimal))
            {
                return false;
            }

            return Value == operand2;
        }
    }
}