using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace BudgetAnalyser.Converters
{
    public static class ConverterHelper
    {
        private const string AccumulatedBucket = "Brush.BudgetBucket.Accumulated";
        private const string CreditBackground1 = "Brush.CreditBackground1";
        private const string CreditBackground2 = "Brush.CreditBackground2";
        private const string DebitBackground1 = "Brush.DebitBackground1";
        private const string DebitBackground2 = "Brush.DebitBackground2";
        private const string IncomeBucket = "Brush.BudgetBucket.Income";
        private const string NeutralNumberBackground = "Brush.NeutralNumberBackground";
        private const string NotSoBadDebit = "Brush.NotSoBadDebit";
        private const string SavingsCommitmentBucket = "Brush.BudgetBucket.SavingsCommittment";
        private const string SecondaryBackground = "Brush.SecondaryBackground";
        private const string SlightDebit = "Brush.SlightDebit";
        private const string SpentMonthlyBucket = "Brush.BudgetBucket.SpentMonthly";
        private const string TileBackground = "Brush.TileBackground";
        private const string TileBackgroundAlternate = "Brush.TileBackgroundAlternate";

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush AccumulatedBucketBrush =
            Application.Current.Resources[AccumulatedBucket] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush CreditBackground1Brush =
            Application.Current.Resources[CreditBackground1] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush CreditBackground2Brush =
            Application.Current.Resources[CreditBackground2] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush DebitBackground1Brush =
            Application.Current.Resources[DebitBackground1] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush DebitBackground2Brush =
            Application.Current.Resources[DebitBackground2] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush IncomeBucketBrush =
            Application.Current.Resources[IncomeBucket] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush NeutralNumberBackgroundBrush =
            Application.Current.Resources[NeutralNumberBackground] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush NotSoBadDebitBrush =
            Application.Current.Resources[NotSoBadDebit] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush SavingsCommitmentBucketBrush =
            Application.Current.Resources[SavingsCommitmentBucket] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush SecondaryBackgroundBrush =
            Application.Current.Resources[SecondaryBackground] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush SlightDebitBrush = Application.Current.Resources[SlightDebit] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush SpentPeriodicallyBucketBrush =
            Application.Current.Resources[SpentMonthlyBucket] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush TileBackgroundBrush =
            Application.Current.Resources[TileBackground] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush TileBackgroundAlternateBrush =
            Application.Current.Resources[TileBackgroundAlternate] as Brush;


        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);

        public static decimal? ParseNumber(object value)
        {
            if (value is decimal)
            {
                return (decimal)value;
            }
            if (value is double)
            {
                var doublenumber = (double)value;
                return Convert.ToDecimal(doublenumber);
            }
            if (value is int)
            {
                var intNumber = (int)value;
                return Convert.ToDecimal(intNumber);
            }
            if (value is long)
            {
                var longNumber = (long)value;
                return Convert.ToDecimal(longNumber);
            }

            var stringValue = value as string;
            if (stringValue != null)
            {
                decimal number;
                if (decimal.TryParse(stringValue, out number))
                {
                    return number;
                }
            }

            return null;
        }
    }
}