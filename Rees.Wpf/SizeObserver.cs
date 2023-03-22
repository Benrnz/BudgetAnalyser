using System.Windows;

namespace Rees.Wpf
{
    /// <summary>
    ///     Sourced from http://stackoverflow.com/questions/1083224/pushing-read-only-gui-properties-back-into-viewmodel
    ///     An Attached behavior that has ObservedWidth and ObservedHeight attached properties. It also has an Observe property
    ///     that is used to do the initial hook-up. Usage looks like this:
    ///     &lt;UserControl ...
    ///     SizeObserver.Observe="True"
    ///     SizeObserver.ObservedWidth="{Binding Width, Mode=OneWayToSource}"
    ///     SizeObserver.ObservedHeight="{Binding Height, Mode=OneWayToSource}"
    ///     So the view model has Width and Height properties that are always in sync with the ObservedWidth and ObservedHeight
    ///     attached properties. The Observe property simply attaches to the SizeChanged event of the FrameworkElement. In the
    ///     handle, it updates its ObservedWidth and ObservedHeight properties. Ergo, the Width and Height of the view model is
    ///     always in sync with the ActualWidth and ActualHeight of the UserControl.
    ///     Perhaps not the perfect solution (I agree - read-only DPs should support OneWayToSource bindings), but it works and
    ///     it upholds the MVVM pattern. Obviously, the ObservedWidth and ObservedHeight DPs are not read-only.
    /// </summary>
    public static class SizeObserver
    {
        public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
                                                                                                        "Observe",
                                                                                                        typeof(bool),
                                                                                                        typeof(SizeObserver),
                                                                                                        new FrameworkPropertyMetadata(OnObserveChanged));

        public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached(
                                                                                                               "ObservedHeight",
                                                                                                               typeof(double),
                                                                                                               typeof(SizeObserver));

        public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached(
                                                                                                              "ObservedWidth",
                                                                                                              typeof(double),
                                                                                                              typeof(SizeObserver));

        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            return (bool)frameworkElement.GetValue(ObserveProperty);
        }

        public static double GetObservedHeight(FrameworkElement frameworkElement)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            return (double)frameworkElement.GetValue(ObservedHeightProperty);
        }

        public static double GetObservedWidth(FrameworkElement frameworkElement)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            return (double)frameworkElement.GetValue(ObservedWidthProperty);
        }

        public static void SetObserve(FrameworkElement frameworkElement, bool observe)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            frameworkElement.SetValue(ObserveProperty, observe);
        }

        public static void SetObservedHeight(FrameworkElement frameworkElement, double observedHeight)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            frameworkElement.SetValue(ObservedHeightProperty, observedHeight);
        }

        public static void SetObservedWidth(FrameworkElement frameworkElement, double observedWidth)
        {
            //frameworkElement.AssertNotNull("frameworkElement");
            frameworkElement.SetValue(ObservedWidthProperty, observedWidth);
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateObservedSizesForFrameworkElement((FrameworkElement)sender);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;

            if ((bool)e.NewValue)
            {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedSizesForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement)
        {
            // WPF 4.0 onwards
            frameworkElement.SetCurrentValue(ObservedWidthProperty, frameworkElement.ActualWidth);
            frameworkElement.SetCurrentValue(ObservedHeightProperty, frameworkElement.ActualHeight);
        }
    }
}