using System.Globalization;

namespace Rees.Wpf.Converters;

/// <summary>
///     If a value is Null will return true, if it is something it will return false.
///     Set <see cref="Invert" /> to true to get the opposite: true when the value is something, false when it is null.
///     Useful to bind IsEnabled properties to the presence of a value a command might be dependent on.
/// </summary>
public class NullToBoolConverter : OneWayValueConverter
{
    /// <summary>
    ///     When true, inverts the result: returns true when the value is something, false when it is null.
    /// </summary>
    public bool Invert { get; set; }

    /// <summary>
    ///     Converts a <see cref="bool" />value.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">Not Used.</param>
    /// <param name="parameter">Not Used.</param>
    /// <param name="culture">Not Used.</param>
    /// <returns>
    ///     Will always return true, or false. False if the value is something, true if it is null (opposite when
    ///     <see cref="Invert" /> is true).
    /// </returns>
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Invert ? value is not null : value is null;
    }
}
