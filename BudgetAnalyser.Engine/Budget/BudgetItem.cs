using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget;

/// <summary>
///     A budget line in a budget. This can represent an expense or an income.
/// </summary>
/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
[DebuggerDisplay("{GetType().Name} {Bucket.Code} ${Amount}")]
public abstract class BudgetItem : INotifyPropertyChanged
{
    private decimal doNotUseAmount;
    private BudgetBucket? doNotUseBucket;

    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets or sets the amount of this budgeted amount.
    /// </summary>
    public decimal Amount
    {
        get => this.doNotUseAmount;

        set
        {
            if (value == this.doNotUseAmount)
            {
                return;
            }

            this.doNotUseAmount = value;
            OnPropertyChanged();
        }
    }


    /// <summary>
    ///     Gets or sets the bucket classification for this.
    /// </summary>
    public required BudgetBucket Bucket
    {
        get => this.doNotUseBucket ?? throw new InvalidOperationException("Bucket is required.");
        set
        {
            if (value == this.doNotUseBucket)
            {
                return;
            }

            if (this.doNotUseBucket is not null)
            {
                this.doNotUseBucket.PropertyChanged -= OnBucketPropertyChanged;
            }

            this.doNotUseBucket = value;
            this.doNotUseBucket.PropertyChanged += OnBucketPropertyChanged;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets the summary.
    /// </summary>
    public string Summary => string.Format(
        CultureInfo.CurrentCulture,
        "{0} {1}: {2}",
        Bucket.TypeDescription.AnOrA(true),
        EnsureNoRepeatedLastWord(Bucket.TypeDescription, GetType().Name),
        Bucket.Description);

    /// <summary>
    ///     Determines whether the specified <see cref="object" />, is equal to this instance.
    ///     Delegates to <see cref="Equals(BudgetItem)" />
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
    /// <returns>
    ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        var other = obj as BudgetItem;
        return other is not null && Equals(other);
    }

    /// <summary>
    ///     Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            return Bucket.GetHashCode() * GetType().GetHashCode();
        }
    }

    /// <summary>
    ///     Implements the operator ==. Delegates to Equals.
    /// </summary>
    public static bool operator ==(BudgetItem left, BudgetItem right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Implements the operator !=. Delegates to Equals.
    /// </summary>
    public static bool operator !=(BudgetItem left, BudgetItem right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    ///     Returns true if the buckets are the same and the types are the same.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    protected bool Equals(BudgetItem other)
    {
        return other is null ? throw new ArgumentNullException(nameof(other)) : Equals(Bucket, other.Bucket) && GetType() == other.GetType();
    }

    /// <summary>
    ///     Called when [property changed].
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static string EnsureNoRepeatedLastWord(string? sentence1, string sentence2)
    {
        if (string.IsNullOrWhiteSpace(sentence1) || string.IsNullOrWhiteSpace(sentence2))
        {
            return string.Empty;
        }

        sentence1 = sentence1.Trim();
        sentence2 = sentence2.Trim();

        string lastWord;
        var wordIndex = sentence1.LastIndexOf(' ');
        lastWord = wordIndex <= 0 ? sentence1 : sentence1.Substring(wordIndex + 1);

        string firstWord;
        wordIndex = sentence2.IndexOf(' ');
        if (wordIndex <= 0)
        {
            firstWord = sentence2;
            wordIndex = firstWord.Length;
        }
        else
        {
            firstWord = sentence2.Substring(0, wordIndex);
        }

        return lastWord == firstWord
            ? string.Format(CultureInfo.CurrentCulture, "{0}{1}", sentence1, sentence2.Substring(wordIndex))
            : string.Format(CultureInfo.CurrentCulture, "{0} {1}", sentence1, sentence2);
    }

    private void OnBucketPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // This is to trigger updates dependent on Bucket, but isn't updated when Active is toggled. For example binding to bucket
        // and using the bucket to colour converter. This converter must return grey when the bucket is inactive.
        if (e.PropertyName == "Active")
        {
            OnPropertyChanged(nameof(Bucket));
        }
    }
}
