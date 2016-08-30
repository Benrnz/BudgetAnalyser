using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Services;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A base class for all widgets. These are used on the Dashboard view to display statistics, metrics, and oil lights.
    /// </summary>
    public abstract class Widget : INotifyPropertyChanged
    {
        /// <summary>
        ///     A constant for displaying the error message: The designed for one month only
        /// </summary>
        protected const string DesignedForOneMonthOnly = "Reduce the date range to one month to enable this widget.";

        /// <summary>
        ///     A constant for the standard widget style. (Blue)
        /// </summary>
        protected const string WidgetStandardStyle = "WidgetStandardStyle";

        /// <summary>
        ///     A constant for an alternative standard widget style. (Green)
        /// </summary>
        protected const string WidgetStandardStyle2 = "WidgetStandardStyle2";

        /// <summary>
        ///     A constant for an alternative standard widget style. (Purple)
        /// </summary>
        protected const string WidgetStandardStyle3 = "WidgetStandardStyle3";

        /// <summary>
        ///     A constant for the warning widget style
        /// </summary>
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
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Reviewed, ok here")]
        protected Widget()
        {
            Name = GetType().Name;
            ColourStyleName = "WidgetStandardBrush";
            Size = WidgetSize.Small;
            WidgetStyle = "ModernTileSmallStyle1";
            Visibility = true;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor - ok here, simple bool property with straightforward usage.
            Enabled = true;
            Sequence = 99;
            RecommendedTimeIntervalUpdate = 30.Seconds();  // TODO - Temporary work around until message refresh issues are resolved.
        }

        /// <summary>
        ///     Occurs when the colour style has changed.
        /// </summary>
        public event EventHandler ColourStyleChanged;

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Occurs when the widget style has changed.
        /// </summary>
        public event EventHandler WidgetStyleChanged;

        /// <summary>
        ///     Gets or sets the grouping category.
        /// </summary>
        public string Category
        {
            get { return this.doNotUseCategory; }
            protected set
            {
                this.doNotUseCategory = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Widget" /> is clickable.
        /// </summary>
        public bool Clickable
        {
            get { return this.doNotUseClickable; }
            set
            {
                this.doNotUseClickable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the name of the colour style.
        /// </summary>
        public string ColourStyleName
        {
            get { return this.doNotUseColour; }
            protected set
            {
                var changed = value != this.doNotUseColour;
                this.doNotUseColour = value;
                OnPropertyChanged();
                if (changed)
                {
                    ColourStyleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the dependencies for this widget to function. See
        ///     <see cref="MonitorableDependencies" /> for a full list of supported dependency types.
        /// </summary>
        public IEnumerable<Type> Dependencies { get; protected set; }

        /// <summary>
        ///     Gets or sets the detailed text to show in the widget UI tile.
        /// </summary>
        public string DetailedText
        {
            get { return this.doNotUseDetailedText; }
            protected set
            {
                this.doNotUseDetailedText = value;
                OnPropertyChanged();
            }
        }

        private bool clickableWhenEnabled;

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Widget" /> is enabled, showing data, and clickable.
        /// </summary>
        public virtual bool Enabled
        {
            get { return this.doNotUseEnabled; }
            protected set
            {
                this.doNotUseEnabled = value;
                OnPropertyChanged();
                if (this.doNotUseEnabled == false && Clickable)
                {
                    this.clickableWhenEnabled = true;
                    Clickable = false;
                } else if (this.doNotUseEnabled == true && !Clickable && this.clickableWhenEnabled)
                {
                    Clickable = true;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the name of the image resource used by this widget.
        /// </summary>
        public string ImageResourceName
        {
            get { return this.doNotUseImageResourceName; }
            protected set
            {
                this.doNotUseImageResourceName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the secondary image resource name.
        /// </summary>
        public string ImageResourceName2
        {
            get { return this.doNotUseImageResourceName2; }
            set
            {
                this.doNotUseImageResourceName2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the large main number used on the widget UI tile.
        /// </summary>
        public string LargeNumber
        {
            get { return this.doNotUseLargeNumber; }
            protected set
            {
                this.doNotUseLargeNumber = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the name of the widget.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     Gets or sets the recommended time interval update.
        /// </summary>
        public TimeSpan? RecommendedTimeIntervalUpdate { get; protected set; }

        /// <summary>
        ///     Gets or sets the sequence used to order the widgets in the UI.
        /// </summary>
        public int Sequence { get; protected set; }

        /// <summary>
        ///     Gets or sets the relative widget size.
        /// </summary>
        protected WidgetSize Size
        {
            get { return this.doNotUseSize; }
            set
            {
                this.doNotUseSize = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the tool tip.
        /// </summary>
        public string ToolTip
        {
            get { return this.doNotUseToolTip; }
            protected set
            {
                this.doNotUseToolTip = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Widget" /> is visibile. Ie: Has the user opted to hide this
        ///     widget.
        /// </summary>
        public bool Visibility
        {
            get { return this.doNotUseVisibility; }
            set
            {
                this.doNotUseVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the widget style name.
        /// </summary>
        public string WidgetStyle
        {
            get { return this.doNotUseWidgetStyle; }
            protected set
            {
                var changed = value != this.doNotUseWidgetStyle;
                this.doNotUseWidgetStyle = value;
                OnPropertyChanged();
                if (changed)
                {
                    WidgetStyleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        public abstract void Update(params object[] input);

        /// <summary>
        ///     Called when a property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Validates the updated input to ensure it is compliant with this widget dependency requirements.
        ///     Called from <see cref="Update" />
        /// </summary>
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
            foreach (var dependencyType in Dependencies)
            {
                var dependencyInstance = input[index++];
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