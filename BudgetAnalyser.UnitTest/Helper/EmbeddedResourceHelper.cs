using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xaml;

namespace BudgetAnalyser.UnitTest.Helper
{
    public static class EmbeddedResourceHelper
    {
        public static T Extract<T>(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    // this line of code is useful to figure out the name Vs has given the resource! The name is case sensitive.
                    Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().ForEach(n => Debug.WriteLine(n));
                    throw new MissingManifestResourceException("Cannot find resource named: " + resourceName);
                }

                return (T)XamlServices.Load(new XamlXmlReader(stream));
            }
        }
    }
}
