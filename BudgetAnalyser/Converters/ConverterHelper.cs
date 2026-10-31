using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace BudgetAnalyser.Converters;

public static class ConverterHelper
{
    internal const string AccumulatedBucket = "Brush.BudgetBucket.Accumulated";
    internal const string CreditBackground1 = "Brush.CreditBackground1";
    internal const string PositiveText = "Brush.Positive.Text";
    internal const string DebitBackground1 = "Brush.DebitBackground1";
    internal const string NegativeText = "Brush.Negative.Text";
    internal const string IncomeBucket = "Brush.BudgetBucket.Income";
    internal const string NeutralNumberBackground = "Brush.NeutralNumberBackground";
    internal const string NotSoBadDebit = "Brush.NotSoBadDebit";
    internal const string SpentMonthlyBucket = "Brush.BudgetBucket.SpentMonthly";
    internal const string TileBackground = "Brush.TileBackground";
    internal const string TileBackgroundAlternate = "Brush.TileBackgroundAlternate";

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? AccumulatedBucketBrush = Application.Current.Resources[AccumulatedBucket] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? CreditBackground1Brush = Application.Current.Resources[CreditBackground1] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? PositiveTextBrush = Application.Current.Resources[PositiveText] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? DebitBackground1Brush = Application.Current.Resources[DebitBackground1] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? NegativeTextBrush = Application.Current.Resources[NegativeText] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? IncomeBucketBrush = Application.Current.Resources[IncomeBucket] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? NeutralNumberBackgroundBrush = Application.Current.Resources[NeutralNumberBackground] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? NotSoBadDebitBrush = Application.Current.Resources[NotSoBadDebit] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? SpentPeriodicallyBucketBrush = Application.Current.Resources[SpentMonthlyBucket] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? TileBackgroundBrush = Application.Current.Resources[TileBackground] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? TileBackgroundAlternateBrush = Application.Current.Resources[TileBackgroundAlternate] as Brush;

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly Brush? TransparentBrush = new SolidColorBrush(Colors.Transparent);

    public static decimal? ParseNumber(object? value)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue;
        }

        if (value is double doubleNumber)
        {
            return Convert.ToDecimal(doubleNumber);
        }

        if (value is int intNumber)
        {
            return Convert.ToDecimal(intNumber);
        }

        if (value is long longNumber)
        {
            return Convert.ToDecimal(longNumber);
        }

        if (value is string stringValue)
        {
            if (decimal.TryParse(stringValue, out var number))
            {
                return number;
            }
        }

        return null;
    }
}
