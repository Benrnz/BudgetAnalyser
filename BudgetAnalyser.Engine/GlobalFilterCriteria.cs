using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    public class GlobalFilterCriteria : INotifyPropertyChanged, IModelValidate, IDataChangeDetection
    {
        private Account.Account doNotUseAccount;
        private DateTime? doNotUseBeginDate;
        private bool doNotUseCleared;
        private DateTime? doNotUseEndDate;

        public GlobalFilterCriteria()
        {
            this.doNotUseCleared = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Account.Account Account
        {
            get { return this.doNotUseAccount; }

            set
            {
                this.doNotUseAccount = value;
                OnPropertyChanged();
                CheckConsistency();
            }
        }

        public DateTime? BeginDate
        {
            get { return this.doNotUseBeginDate; }
            set
            {
                this.doNotUseBeginDate = value;
                OnPropertyChanged();
                CheckConsistency();
            }
        }

        public bool Cleared
        {
            get { return this.doNotUseCleared; }
            private set
            {
                this.doNotUseCleared = value;
                OnPropertyChanged();
            }
        }

        public DateTime? EndDate
        {
            get { return this.doNotUseEndDate; }
            set
            {
                this.doNotUseEndDate = value;
                OnPropertyChanged();
                CheckConsistency();
            }
        }

        public long SignificantDataChangeHash()
        {
            unchecked
            {
                int hashCode = this.doNotUseAccount?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.doNotUseBeginDate.GetHashCode();
                hashCode = (hashCode * 397) ^ this.doNotUseCleared.GetHashCode();
                hashCode = (hashCode * 397) ^ this.doNotUseEndDate.GetHashCode();
                return hashCode;
            }
        }

        public bool Validate([NotNull] StringBuilder validationMessages)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException(nameof(validationMessages));
            }

            if (Cleared)
            {
                BeginDate = null;
                EndDate = null;
                return true;
            }

            var valid = true;
            if (BeginDate == null)
            {
                validationMessages.AppendLine("Begin date cannot be blank unless filter is 'Cleared'.");
                valid = false;
            }

            if (EndDate == null)
            {
                validationMessages.AppendLine("End date cannot be blank unless filter is 'Cleared'.");
                valid = false;
            }

            if (BeginDate > EndDate)
            {
                validationMessages.AppendLine("Begin Date cannot be after the End Date.");
                valid = false;
            }

            return valid;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CheckConsistency()
        {
            if (BeginDate != null && BeginDate.Value == DateTime.MinValue)
            {
                BeginDate = null;
            }

            if (EndDate != null && EndDate.Value == DateTime.MinValue)
            {
                EndDate = null;
            }

            if (BeginDate == null
                && EndDate == null
                && Account == null)
            {
                Cleared = true;
            }
            else
            {
                Cleared = false;
            }

            if (BeginDate != null && EndDate != null && BeginDate > EndDate)
            {
                EndDate = BeginDate.Value.AddDays(1);
            }
        }
    }
}