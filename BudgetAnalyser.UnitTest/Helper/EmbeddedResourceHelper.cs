using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using BudgetAnalyser.UnitTest.TestHarness;
using Portable.Xaml;

namespace BudgetAnalyser.UnitTest.Helper
{
    public static class EmbeddedResourceHelper
    {
        public static IEnumerable<string> ExtractLines(string resourceName, bool outputText = false)
        {
            string contents = ExtractText(resourceName, outputText);
            var reader = new StringReader(contents);
            string line = reader.ReadLine();
            var lines = new List<string>();
            while (line != null)
            {
                lines.Add(line);
                line = reader.ReadLine();
            }

            return lines;
        }

        public static IEnumerable<string> ExtractString(string resourceName)
        {
            using (Stream stream = typeof(FakeLogger).GetTypeInfo().Assembly.GetManifestResourceStream(resourceName))
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

        public static string ExtractText(string resourceName, bool outputText = false)
        {
            using (Stream stream = typeof(FakeLogger).GetTypeInfo().Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    // this line of code is useful to figure out the name Vs has given the resource! The name is case sensitive.
                    ShowAllEmbeddedResources();
                    throw new MissingManifestResourceException("Cannot find resource named: " + resourceName);
                }

                var reader = new StreamReader(stream);
                string stringData = reader.ReadToEnd();
                if (outputText)
                {
                    Debug.WriteLine(stringData);
                }
                return stringData;
            }
        }

        public static T ExtractXaml<T>(string resourceName, bool outputXaml = false)
        {
            string stringData = ExtractText(resourceName, outputXaml);
            return (T)XamlServices.Parse(stringData);
        }

        private static void ShowAllEmbeddedResources()
        {
            typeof(FakeLogger).GetTypeInfo().Assembly.GetManifestResourceNames().ToList().ForEach(n => Debug.WriteLine(n));
        }
    }
}