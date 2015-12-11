using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    /// <summary>
    ///     A class used to store a criteria value when creating a new <see cref="MatchingRule" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Criteria<T> : INotifyPropertyChanged 
    {
        private bool doNotUseApplicable;
        private T doNotUseValue;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public abstract bool IsEqualButNotBlank(T operand2);

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class StringCriteria : Criteria<string>
    {
        public override bool IsEqualButNotBlank(string operand2)
        {
            if (string.IsNullOrWhiteSpace(Value) || string.IsNullOrWhiteSpace(operand2))
            {
                return false;
            }

            bool match = Value == operand2;
            return match;
        }
    }

    public class DecimalCriteria : Criteria<decimal?>
    {
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