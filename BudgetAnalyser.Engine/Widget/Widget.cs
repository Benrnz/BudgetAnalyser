using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Widget
{
    public abstract class Widget : INotifyPropertyChanged
    {
        protected const string WidgetStandardBrush = "WidgetWarningBrush";
        protected const string WidgetWarningBrush = "WidgetWarningBrush";

        private string doNotUseCategory;
        private string doNotUseColour;
        private string doNotUseDetailedText;
        private string doNotUseImageResourceUri;
        private string doNotUseLargeNumber;
        private string doNotUseToolTip;
        private bool doNotUseVisibility;

        protected Widget()
        {
            Name = GetType().Name;
            ColourStyleName = "WidgetStandardBrush";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler StyleChanged;

        public string Category
        {
            get { return this.doNotUseCategory; }
            protected set
            {
                this.doNotUseCategory = value;
                OnPropertyChanged();
            }
        }

        public string ColourStyleName
        {
            get { return this.doNotUseColour; }
            protected set
            {
                bool changed = value != this.doNotUseColour;
                this.doNotUseColour = value;
                OnPropertyChanged();
                if (changed)
                {
                    OnStyleChanged();
                }
            }
        }

        public object Command { get; set; }
        public IEnumerable<Type> Dependencies { get; protected set; }

        public string DetailedText
        {
            get { return this.doNotUseDetailedText; }
            protected set
            {
                this.doNotUseDetailedText = value;
                OnPropertyChanged();
            }
        }

        public string ImageResourceUri
        {
            get { return this.doNotUseImageResourceUri; }
            protected set
            {
                this.doNotUseImageResourceUri = value;
                OnPropertyChanged();
            }
        }

        public string LargeNumber
        {
            get { return this.doNotUseLargeNumber; }
            protected set
            {
                this.doNotUseLargeNumber = value;
                OnPropertyChanged();
            }
        }

        public string Name { get; protected set; }

        public TimeSpan? RecommendedTimeIntervalUpdate { get; protected set; }

        public string ToolTip
        {
            get { return this.doNotUseToolTip; }
            protected set
            {
                this.doNotUseToolTip = value;
                OnPropertyChanged();
            }
        }

        public bool Visibility
        {
            get { return this.doNotUseVisibility; }
            protected set
            {
                this.doNotUseVisibility = value;
                OnPropertyChanged();
            }
        }

        public abstract void Update(params object[] input);

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnStyleChanged()
        {
            EventHandler handler = StyleChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected bool ValidateUpdateInput(object[] input)
        {
            if (input == null)
            {
                return false;
            }

            List<Type> dependencies = Dependencies.ToList();
            if (dependencies.Count() > input.Length)
            {
                return false;
            }

            int index = 0;
            foreach (Type dependencyType in Dependencies)
            {
                object dependencyInstance = input[index++];
                if (dependencyInstance == null)
                {
                    return false;
                }

                if (!dependencyType.IsInstanceOfType(dependencyInstance))
                {
                    return false;
                }
            }

            return true;
        }
    }
}