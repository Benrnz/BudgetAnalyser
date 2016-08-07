using System.Reflection;
using System.Xaml;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Helper
{
    /// <summary>
    ///     An additional XAML focused overload over and above what is provided by Rees.UnitTest.Utilities.
    /// </summary>
    public static class EmbeddedResourceHelper
    {
        /// <summary>
        ///     Accesses a embedded resource xaml file included in the assembly and returns it as a deserialised object.
        /// </summary>
        /// <typeparam name="T">The type of the object described in the xaml file. This is used to deserialise the object.</typeparam>
        /// <param name="assembly">The assembly in which to find the embedded resource.</param>
        /// <param name="resourceName">
        ///     The resource name of the file. This matches its filename and path from the root of the solution.
        ///     For example "AssemblyName.FolderName(s).EmbeddedFile.xaml";
        /// </param>
        /// <param name="outputXaml">Optionally output the xaml file to the Debug console before returning.</param>
        /// <returns>A fully deserialised object.</returns>
        public static T ExtractEmbeddedResourceAsXamlObject<T>(this Assembly assembly, string resourceName, bool outputXaml = false)
        {
            var stringData = assembly.ExtractEmbeddedResourceAsText(resourceName, outputXaml);
            return (T) XamlServices.Parse(stringData);
        }
    }
}