using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace BudgetAnalyser.Converters
{
    public static class ConverterHelper
    {
        private const string AccumulatedBucket = "Brush.BudgetBucket.Accumulated";
        private const string CreditBackground1 = "Brush.CreditBackground1";
        private const string DebitBackground1 = "Brush.DebitBackground1";

        private const string IncomeBucket = "Brush.BudgetBucket.Income";

        private const string NeutralNumberBackground = "Brush.NeutralNumberBackground";

        private const string NotSoBadDebit = "Brush.NotSoBadDebit";
        private const string SecondaryBackground = "Brush.SecondaryBackground";

        private const string SlightDebit = "Brush.SlightDebit";
        private const string SpentMonthlyBucket = "Brush.BudgetBucket.SpentMonthly";
        private const string TileBackground = "Brush.TileBackground";
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush AccumulatedBucketBrush = Application.Current.Resources[AccumulatedBucket] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush CreditBackground1Brush =
            Application.Current.Resources[CreditBackground1] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush DebitBackground1Brush =
            Application.Current.Resources[DebitBackground1] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush IncomeBucketBrush = Application.Current.Resources[IncomeBucket] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush NeutralNumberBackgroundBrush =
            Application.Current.Resources[NeutralNumberBackground] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush NotSoBadDebitBrush =
            Application.Current.Resources[NotSoBadDebit] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush SecondaryBackgroundBrush =
            Application.Current.Resources[SecondaryBackground] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush SlightDebitBrush = Application.Current.Resources[SlightDebit] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush SpentMonthlyBucketBrush = Application.Current.Resources[SpentMonthlyBucket] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush TileBackgroundBrush =
            Application.Current.Resources[TileBackground] as Brush;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
    }
}