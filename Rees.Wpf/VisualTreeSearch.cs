using System.Windows.Media;

namespace Rees.Wpf
{
    /// <summary>
    ///     A helper class to search the visual tree.
    /// </summary>
    public class VisualTreeSearch
    {
        /// <summary>
        ///     A helper method to search the visual tree for a child of the <paramref name="parent" /> of a certain type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public T GetVisualChild<T>(Visual parent) where T : Visual
        {
            var child = default(T);
            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }

                if (child != null)
                {
                    break;
                }
            }

            return child;
        }
    }
}