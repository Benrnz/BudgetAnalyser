using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BackgroundProcessingJobMetadata : IBackgroundProcessingJobMetadata, INotifyPropertyChanged
    {
        private string doNotUseDescription;
        private bool jobRunning;
        private bool doNotUseMenuAvailable;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public BackgroundProcessingJobMetadata()
        {
            MenuAvailable = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Description
        {
            get { return this.doNotUseDescription; }
            private set
            {
                this.doNotUseDescription = value;
                OnPropertyChanged();
            }
        }

        public bool MenuAvailable
        {
            get { return this.doNotUseMenuAvailable; }
            private set
            {
                this.doNotUseMenuAvailable = value;
                OnPropertyChanged();
            }
        }

        public void Finish()
        {
            if (!this.jobRunning)
            {
                throw new InvalidOperationException("A background job isn't running, invalid call to Finish.");
            }

            Description = null;
            MenuAvailable = true;
            this.jobRunning = false;
        }

        public void StartNew(string description, bool menuAvailable)
        {
            if (this.jobRunning)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "A Background job is already running ({0}), and another was attempting to start ({1}).", Description, description));
            }

            Description = description;
            MenuAvailable = menuAvailable;
            this.jobRunning = true;
        }

        [NotifyPropertyChangedInvocator]
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