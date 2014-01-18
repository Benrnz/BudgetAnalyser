using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Engine
{
    public class GlobalFilterCriteria : INotifyPropertyChanged, IModelValidate
    {
        private DateTime? doNotUseBeginDate;
        private bool doNotUseCleared;
        private DateTime? doNotUseEndDate;
        private AccountType doNotUseAccountType;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public AccountType AccountType
        {
            get { return this.doNotUseAccountType; }

            set
            {
                this.doNotUseAccountType = value;
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

        public string Description
        {
            get
            {
                if (Cleared)
                {
                    return "Showing all.";
                }

                return string.Format(CultureInfo.CurrentCulture, "From {0:d} to {1:d}.", BeginDate, EndDate);
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

        public bool Validate(StringBuilder validationMessages)
        {
            if (Cleared)
            {
                BeginDate = null;
                EndDate = null;
                return true;
            }

            bool valid = true;
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

            return valid;
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
                && AccountType == null)
            {
                Cleared = true;
            }
            else
            {
                Cleared = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}