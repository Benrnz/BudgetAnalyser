using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A base class for all widgets. These are used on the Dashboard view to display statistics, metrics, and oil lights.
    /// </summary>
    public abstract class Widget : INotifyPropertyChanged
    {
        protected const string DesignedForOneMonthOnly = "Reduce the date range to one month to enable this widget.";
        protected const string WidgetStandardStyle = "WidgetStandardStyle";
        protected const string WidgetWarningStyle = "WidgetWarningStyle";
        private string doNotUseCategory;
        private bool doNotUseClickable;
        private string doNotUseColour;
        private string doNotUseDetailedText;
        private bool doNotUseEnabled;
        private string doNotUseImageResourceName;
        private string doNotUseImageResourceName2;
        private string doNotUseLargeNumber;
        private WidgetSize doNotUseSize;
        private string doNotUseToolTip;
        private bool doNotUseVisibility;
        private string doNotUseWidgetStyle;

        /// <summary>
        ///     Constructs a new instance of the <see cref="Widget" /> class.
        ///     All widgets must have a parameterless constructor. This is the constructor that will be used to create all widgets.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Reviewed, ok here")]
        protected Widget()
        {
            Name = GetType().Name;
            ColourStyleName = "WidgetStandardBrush";
            Size = WidgetSize.Small;
            WidgetStyle = "ModernTileSmallStyle1";
            Visibility = true;
            Enabled = true;
            Sequence = 99;
        }

        public event EventHandler ColourStyleChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler WidgetStyleChanged;

        public string Category
        {
            get { return this.doNotUseCategory; }
            protected set
            {
                this.doNotUseCategory = value;
                OnPropertyChanged();
            }
        }

        public bool Clickable
        {
            get { return this.doNotUseClickable; }
            set
            {
                this.doNotUseClickable = value;
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
                    OnStyleChanged(ColourStyleChanged);
                }
            }
        }

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

        public virtual bool Enabled
        {
            get { return this.doNotUseEnabled; }
            set
            {
                this.doNotUseEnabled = value;
                OnPropertyChanged();
            }
        }

        public string ImageResourceName
        {
            get { return this.doNotUseImageResourceName; }
            protected set
            {
                this.doNotUseImageResourceName = value;
                OnPropertyChanged();
            }
        }

        public string ImageResourceName2
        {
            get { return this.doNotUseImageResourceName2; }
            set
            {
                this.doNotUseImageResourceName2 = value;
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
        public int Sequence { get; protected set; }

        public WidgetSize Size
        {
            get { return this.doNotUseSize; }
            protected set
            {
                this.doNotUseSize = value;
                OnPropertyChanged();
            }
        }

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
            set
            {
                this.doNotUseVisibility = value;
                OnPropertyChanged();
            }
        }

        public string WidgetStyle
        {
            get { return this.doNotUseWidgetStyle; }
            protected set
            {
                bool changed = value != this.doNotUseWidgetStyle;
                this.doNotUseWidgetStyle = value;
                OnPropertyChanged();
                if (changed)
                {
                    OnStyleChanged(WidgetStyleChanged);
                }
            }
        }

        public abstract void Update(params object[] input);

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnStyleChanged(EventHandler eventToInvoke)
        {
            EventHandler handler = eventToInvoke;
            handler?.Invoke(this, EventArgs.Empty);
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

            int index = 0, nullCount = 0;
            foreach (Type dependencyType in Dependencies)
            {
                object dependencyInstance = input[index++];
                if (dependencyInstance == null)
                {
                    // Allow this to continue, because nulls are valid when the dependency isnt available yet.
                    nullCount++;
                    continue;
                }

                if (!dependencyType.IsInstanceOfType(dependencyInstance))
                {
                    return false;
                }
            }

            if (nullCount == Dependencies.Count())
            {
                return false;
            }

            return true;
        }
    }
}