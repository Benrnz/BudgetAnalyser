using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BudgetAnalyser.UI;

/// <summary>
///     Attached properties for TextBoxes used to enter numbers.
/// </summary>
public static class NumberTextBox
{
    /// <summary>
    ///     When true, typing a second decimal point into the TextBox is ignored.
    /// </summary>
    public static readonly DependencyProperty SingleDecimalPointProperty = DependencyProperty.RegisterAttached(
        "SingleDecimalPoint",
        typeof(bool),
        typeof(NumberTextBox),
        new PropertyMetadata(false, OnSingleDecimalPointChanged));

    public static bool GetSingleDecimalPoint(DependencyObject element)
    {
        return (bool)element.GetValue(SingleDecimalPointProperty);
    }

    public static void SetSingleDecimalPoint(DependencyObject element, bool value)
    {
        element.SetValue(SingleDecimalPointProperty, value);
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = (TextBox)sender;
        var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        if (!e.Text.Contains(separator, StringComparison.Ordinal))
        {
            return;
        }

        // Typing over a selection replaces it, so a decimal point inside the selection doesn't count.
        var remainingText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
        if (remainingText.Contains(separator, StringComparison.Ordinal))
        {
            e.Handled = true;
        }
    }

    private static void OnSingleDecimalPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        textBox.PreviewTextInput -= OnPreviewTextInput;
        if ((bool)e.NewValue)
        {
            textBox.PreviewTextInput += OnPreviewTextInput;
        }
    }
}
