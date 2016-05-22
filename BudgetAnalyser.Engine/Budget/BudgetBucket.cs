using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A "bucket" that represents a place to budget some funds for a specific purpose.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.IModelValidate" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.IComparable" />
    [DebuggerDisplay("{TypeDescription} {Code} {Description}")]
    public abstract class BudgetBucket : IModelValidate, INotifyPropertyChanged, IComparable
    {
        private bool doNotUseActive;
        private string doNotUseCode;
        private string doNotUseDescription;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BudgetBucket" /> class.
        /// </summary>
        protected BudgetBucket()
        {
            this.doNotUseActive = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BudgetBucket" /> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        protected BudgetBucket(string code, string name) : this()
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.doNotUseDescription = name;
            this.doNotUseCode = code.ToUpperInvariant();
        }

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="BudgetBucket" /> is active.
        ///     If Inactive the Bucket will not be used in auto-matching nor available for manual transation matching.
        /// </summary>
        /// <value>
        ///     <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        public bool Active
        {
            get { return this.doNotUseActive; }
            set
            {
                this.doNotUseActive = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a textual code that uniquely identifies this bucket.
        /// </summary>
        public string Code
        {
            get { return this.doNotUseCode; }

            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                this.doNotUseCode = value.ToUpperInvariant();
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the description of this bucket.
        /// </summary>
        public string Description
        {
            get { return this.doNotUseDescription; }
            set
            {
                this.doNotUseDescription = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets a description of this type of bucket. By default this is the <see cref="System.Type.Name" />
        /// </summary>
        /// <value>
        ///     The type description.
        /// </value>
        public virtual string TypeDescription => GetType().Name;

        /// <summary>
        ///     Compares two <see cref="BudgetBucket" /> instances and returns an <see cref="int" />.  If the value is less than
        ///     zero the first operand is less than the second.
        ///     Comparisions are performed using the <see cref="Code" /> property which uniquely identifies a bucket.
        /// </summary>
        public int CompareTo(object obj)
        {
            var otherBucket = obj as BudgetBucket;
            if (otherBucket == null)
            {
                return -1;
            }

            return string.Compare(Code, otherBucket.Code, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Validate the instance and populate any warnings and errors into the <paramref name="validationMessages" /> string
        ///     builder.
        /// </summary>
        /// <param name="validationMessages">A non-null string builder that will be appended to for any messages.</param>
        /// <returns>
        ///     If the instance is in an invalid state it will return false, otherwise it returns true.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Validate([NotNull] StringBuilder validationMessages)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException(nameof(validationMessages));
            }

            var retval = true;
            if (string.IsNullOrWhiteSpace(Code))
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture,
                    "Budget Bucket {0} is invalid, Code must be a small textual code.", Code);
                retval = false;
            }
            else
            {
                if (Code.Length > 7)
                {
                    validationMessages.AppendFormat(CultureInfo.CurrentCulture,
                        "Budget Bucket {0} - {1} is invalid, Code must be a small textual code less than 7 characters.",
                        Code, Description);
                    retval = false;
                }
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture,
                    "Budget Bucket {0} is invalid, Description must not be blank.", Code);
                retval = false;
            }

            return retval;
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        ///     Comparisons are performed using the <see cref="Code" /> Property.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var otherBucket = obj as BudgetBucket;
            if (otherBucket == null)
            {
                return false;
            }

            return Code == otherBucket.Code;
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        /// <summary>
        ///     Implements the operator ==. Delegates to Equals.
        /// </summary>
        public static bool operator ==(BudgetBucket obj1, BudgetBucket obj2)
        {
            object obj3 = obj1, obj4 = obj2;
            if (obj3 == null && obj4 == null)
            {
                return true;
            }

            if (obj3 == null || obj4 == null)
            {
                return false;
            }

            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            return obj1.Equals(obj2);
        }

        /// <summary>
        ///     Implements the operator &gt;. Delegates to CompareTo
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static bool operator >([NotNull] BudgetBucket obj1, [NotNull] BudgetBucket obj2)
        {
            if (obj1 == null)
            {
                throw new ArgumentNullException(nameof(obj1));
            }

            if (obj2 == null)
            {
                throw new ArgumentNullException(nameof(obj2));
            }

            return obj1.CompareTo(obj2) > 0;
        }

        /// <summary>
        ///     Implements the operator !=. Delegates to ==
        /// </summary>
        public static bool operator !=(BudgetBucket obj1, BudgetBucket obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        ///     Implements the operator &lt;. Delegates to CompareTo
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static bool operator <([NotNull] BudgetBucket obj1, [NotNull] BudgetBucket obj2)
        {
            if (obj1 == null)
            {
                throw new ArgumentNullException(nameof(obj1));
            }

            if (obj2 == null)
            {
                throw new ArgumentNullException(nameof(obj2));
            }

            return obj1.CompareTo(obj2) < 0;
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[{0}] {1}", Code, Description);
        }

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}