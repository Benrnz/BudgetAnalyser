using System.Windows;
using System.Windows.Media;

namespace BudgetAnalyser.Converters
{
    public static class ConverterHelper
    {
        private const string CreditBackground1 = "Brush.CreditBackground1";
        private const string DebitBackground1 = "Brush.DebitBackground1";

        private const string DebitBackground2 = "Brush.DebitBackground2";

        private const string TileBackground = "Brush.TileBackground";

        private const string NeutralNumberBackground = "Brush.NeutralNumberBackground";

        private const string NotSoBadDebit = "Brush.NotSoBadDebit";
        private const string SecondaryBackground = "Brush.SecondaryBackground";

        private const string SlightDebit = "Brush.SlightDebit";

        public static readonly Brush CreditBackground1Brush = Application.Current.Resources[CreditBackground1] as Brush;

        public static readonly Brush DebitBackground1Brush = Application.Current.Resources[DebitBackground1] as Brush;

        public static readonly Brush DebitBackground2Brush = Application.Current.Resources[DebitBackground2] as Brush;

        public static readonly Brush NeutralNumberBackgroundBrush = Application.Current.Resources[NeutralNumberBackground] as Brush;
        public static readonly Brush NotSoBadDebitBrush = Application.Current.Resources[NotSoBadDebit] as Brush;
        public static readonly Brush SecondaryBackgroundBrush = Application.Current.Resources[SecondaryBackground] as Brush;
        public static readonly Brush SlightDebitBrush = Application.Current.Resources[SlightDebit] as Brush;
        public static readonly Brush TileBackgroundBrush = Application.Current.Resources[TileBackground] as Brush;

        public static readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
    }
}