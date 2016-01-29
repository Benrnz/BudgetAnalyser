using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;
using Portable.Xaml;

namespace BudgetAnalyser.Uwp.ApplicationState
{
    /// <summary>
    ///     An implmentation of <see cref="IPersistApplicationState" /> that saves the user meta-data as Xaml to a file on the
    ///     local disk.
    /// </summary>
    public class PersistApplicationStateAsXaml : IPersistApplicationState
    {
        private readonly IEnvironmentFolders folders;
        private const string FileName = "BudgetAnalyserAppState.xml";

        private string doNotUseFullFileName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistApplicationStateAsXaml" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">userMessageBox cannot be null.</exception>
        public PersistApplicationStateAsXaml([NotNull] IEnvironmentFolders folders)
        {
            if (folders == null) throw new ArgumentNullException(nameof(folders));
            this.folders = folders;
        }

        /// <summary>
        ///     Gets the full name of the file to save the data into.
        ///     The file will be overwritten.
        ///     By default this will save to the application folder with the name BudgetAnalyserAppState.xml.
        /// </summary>
        protected virtual async Task<string> FullFileName()
        {
            if (string.IsNullOrEmpty(this.doNotUseFullFileName))
            {
                var location = await this.folders.ApplicationDataFolder();
                Debug.Assert(location != null);
                this.doNotUseFullFileName = Path.Combine(location, FileName);
            }

            return this.doNotUseFullFileName;
        }

        /// <summary>
        ///     Load the user state from the Xaml file on the local disk.
        /// </summary>
        /// <returns>
        ///     An array of data objects that are self identifying. This array will need to be processed or broadcasted to the
        ///     components that consume this data.
        /// </returns>
        /// <exception cref="BadApplicationStateFileFormatException">
        ///     This will be thrown if the file is invalid.
        /// </exception>
        public async Task<IEnumerable<IPersistentApplicationState>> LoadAsync()
        {
            var fullFileName = await FullFileName();
            if (!File.Exists(fullFileName))
            {
                return new List<IPersistentApplicationState>();
            }

            object serialised = XamlServices.Load(fullFileName);
            // Will always succeed without exceptions even if bad file format, but will return null.
            var correctFormat = serialised as List<IPersistentApplicationState>;
            if (correctFormat == null)
            {
                throw new BadApplicationStateFileFormatException($"The file used to store application state ({fullFileName}) is not in the correct format. It may have been tampered with.");
            }

            return correctFormat;
        }

        /// <summary>
        ///     Persist the user data to the Xaml file on the local disk.
        /// </summary>
        /// <param name="modelsToPersist">
        ///     All components in the App that implement <see cref="IPersistentApplicationState" /> so
        ///     the implementation can go get the data to persist.
        /// </param>
        public async Task PersistAsync(IEnumerable<IPersistentApplicationState> modelsToPersist)
        {
            var data = new List<IPersistentApplicationState>(modelsToPersist.ToList());
            var serialised = XamlServices.Save(data);
            var fullFileName = await FullFileName();
            using (var file = new FileStream(fullFileName, FileMode.Create))
            {
                using (var writer = new StreamWriter(file))
                {
                    await writer.WriteAsync(serialised);
                }
            }
        }
    }
}