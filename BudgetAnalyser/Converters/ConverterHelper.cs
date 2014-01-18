using System.Windows;
using System.Windows.Media;

namespace BudgetAnalyser.Converters
{
    public static class ConverterHelper
    {
        private const string CreditBackground1 = "CreditBackground1Brush";
        private const string DebitBackground1 = "DebitBackground1Brush";

        private const string DebitBackground2 = "DebitBackground2Brush";

        private const string NeutralNumberBackground = "NeutralNumberBackgroundBrush";

        private const string NotSoBadDebit = "NotSoBadDebitBrush";
        private const string SecondaryBackground = "SecondaryBackgroundBrush";

        private const string SlightDebit = "SlightDebitBrush";

        private const string TertiaryBackground = "TertiaryBackgroundBrush";

        public static readonly Brush CreditBackground1Brush = Application.Current.Resources[CreditBackground1] as Brush;

        public static readonly Brush DebitBackground1Brush = Application.Current.Resources[DebitBackground1] as Brush;

        public static readonly Brush DebitBackground2Brush = Application.Current.Resources[DebitBackground2] as Brush;

        public static readonly Brush NeutralNumberBackgroundBrush = Application.Current.Resources[NeutralNumberBackground] as Brush;
        public static readonly Brush NotSoBadDebitBrush = Application.Current.Resources[NotSoBadDebit] as Brush;
        public static readonly Brush SecondaryBackgroundBrush = Application.Current.Resources[SecondaryBackground] as Brush;
        public static readonly Brush SlightDebitBrush = Application.Current.Resources[SlightDebit] as Brush;
        public static readonly Brush TertiaryBackgroundBrush = Application.Current.Resources[TertiaryBackground] as Brush;

        public static readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
    }
}