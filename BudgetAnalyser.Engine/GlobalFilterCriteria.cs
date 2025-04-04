﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace BudgetAnalyser.Engine;

/// <summary>
///     A set of criteria for filtering all budget data.
/// </summary>
public class GlobalFilterCriteria : INotifyPropertyChanged, IModelValidate, IDataChangeDetection
{
    private DateOnly? doNotUseBeginDate;
    private bool doNotUseCleared;
    private DateOnly? doNotUseEndDate;

    /// <summary>
    ///     Constructs a new instance of <see cref="GlobalFilterCriteria" />
    /// </summary>
    public GlobalFilterCriteria()
    {
        this.doNotUseCleared = true;
    }

    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     The earliest date to include in filtered data when the criteria is applied. This is inclusive of the date.
    /// </summary>
    public DateOnly? BeginDate
    {
        get => this.doNotUseBeginDate;
        set
        {
            if (value == this.doNotUseBeginDate)
            {
                return;
            }

            this.doNotUseBeginDate = value;
            OnPropertyChanged();
            CheckConsistency();
        }
    }

    /// <summary>
    ///     An automatically set property indicating if this criteria instance contains no filtering criteria; so if applied will include all data.
    /// </summary>
    public bool Cleared
    {
        get => this.doNotUseCleared;
        private set
        {
            this.doNotUseCleared = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     The latest date to include in filtered data when this criteria is applied. The date is exclusive.
    /// </summary>
    public DateOnly? EndDate
    {
        get => this.doNotUseEndDate;
        set
        {
            if (value == this.doNotUseEndDate)
            {
                return;
            }

            this.doNotUseEndDate = value;
            OnPropertyChanged();
            CheckConsistency();
        }
    }

    /// <summary>
    ///     Calculates a hash that represents a data state for the criteria. Different criteria, will result in a different hash.
    /// </summary>
    public long SignificantDataChangeHash()
    {
        unchecked
        {
            var hashCode = this.doNotUseBeginDate.GetHashCode();
            hashCode = (hashCode * 397) ^ this.doNotUseCleared.GetHashCode();
            hashCode = (hashCode * 397) ^ this.doNotUseEndDate.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    ///     Validate the instance and populate any warnings and errors into the <paramref name="validationMessages" /> string builder.
    /// </summary>
    /// <param name="validationMessages">A non-null string builder that will be appended to for any messages.</param>
    /// <returns>If the instance is in an invalid state it will return false, otherwise it returns true.</returns>
    public bool Validate(StringBuilder validationMessages)
    {
        if (validationMessages is null)
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
        if (BeginDate is null)
        {
            validationMessages.AppendLine("Begin date cannot be blank unless filter is 'Cleared'.");
            valid = false;
        }

        if (EndDate is null)
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

    private void CheckConsistency()
    {
        if (BeginDate is not null && BeginDate.Value == DateOnly.MinValue)
        {
            BeginDate = null;
        }

        if (EndDate is not null && EndDate.Value == DateOnly.MinValue)
        {
            EndDate = null;
        }

        Cleared = BeginDate is null && EndDate is null;

        if (BeginDate is not null && EndDate is not null && BeginDate > EndDate)
        {
            EndDate = BeginDate.Value.AddDays(1);
        }
    }

    /// <summary>
    ///     Raise the property change event.
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
