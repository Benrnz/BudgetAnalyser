using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace BudgetAnalyser.Converters;

public static class ConverterHelper
{
    private const string AccumulatedBucket = "Brush.BudgetBucket.Accumulated";
    private const string CreditBackground1 = "Brush.CreditBackground1";
    private const string PositiveText = "Brush.Positive.Text";
    private const string DebitBackground1 = "Brush.DebitBackground1";
    private const string NegativeText = "Brush.Negative.Text";
    private const string IncomeBucket = "Brush.BudgetBucket.Income";
    private const string NeutralNumberBackground = "Brush.NeutralNumberBackground";
    private const string NotSoBadDebit = "Brush.NotSoBadDebit";
    private const string SecondaryBackground = "Brush.SecondaryBackground";
    private const string SpentMonthlyBucket = "Brush.BudgetBucket.SpentMonthly";
    private const string TileBackground = "Brush.TileBackground";
    private const string TileBackgroundAlternate = "Brush.TileBackgroundAlternate";

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
    public static readonly Brush? SecondaryBackgroundBrush = Application.Current.Resources[SecondaryBackground] as Brush;

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
