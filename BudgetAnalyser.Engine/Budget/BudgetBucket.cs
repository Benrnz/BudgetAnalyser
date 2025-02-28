﻿using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget;

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
    private string doNotUseCode = string.Empty;
    private string doNotUseDescription = string.Empty;

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
        if (code is null)
        {
            throw new ArgumentNullException(nameof(code));
        }

        this.doNotUseDescription = name ?? throw new ArgumentNullException(nameof(name));
        this.doNotUseCode = code.ToUpperInvariant();
    }

    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="BudgetBucket" /> is active.
    ///     If Inactive the Bucket will not be used in auto-matching nor available for manual transation matching.
    /// </summary>
    /// <value>
    ///     <c>true</c> if active; otherwise, <c>false</c>.
    /// </value>
    public bool Active
    {
        get => this.doNotUseActive;
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
        get => this.doNotUseCode;

        set
        {
            if (value == this.doNotUseCode)
            {
                return;
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
        get => this.doNotUseDescription;
        set
        {
            if (value == this.doNotUseDescription)
            {
                return;
            }

            this.doNotUseDescription = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets a description of this type of bucket. By default, this is the <see cref="System.Type.Name" />
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
    public int CompareTo(object? obj)
    {
        return obj is not BudgetBucket otherBucket ? -1 : string.Compare(Code, otherBucket.Code, StringComparison.Ordinal);
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
    public bool Validate(StringBuilder validationMessages)
    {
        if (validationMessages is null)
        {
            throw new ArgumentNullException(nameof(validationMessages));
        }

        var result = true;
        if (string.IsNullOrWhiteSpace(Code))
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Budget Bucket {0} is invalid, Code must be a small textual code.", Code);
            result = false;
        }
        else
        {
            if (Code.Length > 7)
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Budget Bucket {0} - {1} is invalid, Code must be a small textual code less than 7 characters.", Code, Description);
                result = false;
            }
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Budget Bucket {0} is invalid, Description must not be blank.", Code);
            result = false;
        }

        return result;
    }

    /// <summary>
    ///     Determines whether the specified <see cref="object" />, is equal to this instance.
    ///     Comparisons are performed using the <see cref="Code" /> Property.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns>
    ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is BudgetBucket otherBucket && Code == otherBucket.Code;
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
    public static bool operator ==(BudgetBucket? obj1, BudgetBucket? obj2)
    {
        object? obj3 = obj1, obj4 = obj2;
        if (obj3 is null && obj4 is null)
        {
            return true;
        }

        if (obj3 is null || obj4 is null)
        {
            return false;
        }

        return ReferenceEquals(obj1, obj2) || obj1!.Equals(obj2);
    }

    /// <summary>
    ///     Implements the operator &gt;. Delegates to CompareTo
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// </exception>
    public static bool operator >(BudgetBucket obj1, BudgetBucket obj2)
    {
        if (obj1 is null)
        {
            throw new ArgumentNullException(nameof(obj1));
        }

        return obj2 is null ? throw new ArgumentNullException(nameof(obj2)) : obj1.CompareTo(obj2) > 0;
    }

    /// <summary>
    ///     Implements the operator !=. Delegates to ==
    /// </summary>
    public static bool operator !=(BudgetBucket? obj1, BudgetBucket? obj2)
    {
        return !(obj1 == obj2);
    }

    /// <summary>
    ///     Implements the operator &lt;. Delegates to CompareTo
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// </exception>
    public static bool operator <(BudgetBucket obj1, BudgetBucket obj2)
    {
        if (obj1 is null)
        {
            throw new ArgumentNullException(nameof(obj1));
        }

        return obj2 is null ? throw new ArgumentNullException(nameof(obj2)) : obj1.CompareTo(obj2) < 0;
    }

    /// <summary>
    ///     Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"[{Code}] {Description}";
    }

    /// <summary>
    ///     Called when [property changed].
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
