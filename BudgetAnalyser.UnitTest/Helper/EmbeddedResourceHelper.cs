using System;
using System.Collections.Generic;
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
        public static IEnumerable<string> ExtractString(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    // this line of code is useful to figure out the name Vs has given the resource! The name is case sensitive.
                    ShowAllEmbeddedResources();
                    throw new MissingManifestResourceException("Cannot find resource named: " + resourceName);
                }

                using (var streamReader = new StreamReader(stream))
                {
                    string text = streamReader.ReadToEnd();
                    return text.Split('\n');
                }
            }
        }

        public static T ExtractXaml<T>(string resourceName, bool outputXaml = false)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    // this line of code is useful to figure out the name Vs has given the resource! The name is case sensitive.
                    ShowAllEmbeddedResources();
                    throw new MissingManifestResourceException("Cannot find resource named: " + resourceName);
                }

                var reader = new StreamReader(stream);
                string stringData = reader.ReadToEnd();
                if (outputXaml) Debug.WriteLine(stringData);
                return (T)XamlServices.Parse(stringData);
            }
        }

        private static void ShowAllEmbeddedResources()
        {
            Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().ForEach(n => Debug.WriteLine(n));
        }
    }
}