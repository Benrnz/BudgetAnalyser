using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xaml;
using Rees.Wpf.Annotations;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.ApplicationState
{
    /// <summary>
    /// An implmentation of <see cref="IPersistApplicationState"/> that saves the user meta-data as Xaml to a file on the local disk.
    /// </summary>
    public class PersistApplicationStateAsXaml : IPersistApplicationState
    {
        private const string FileName = "BudgetAnalyserAppState.xml";
        private readonly IUserMessageBox userMessageBox;

        private string doNotUseFullFileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistApplicationStateAsXaml"/> class.
        /// </summary>
        /// <param name="userMessageBox">A service to show the user a message box.</param>
        /// <exception cref="System.ArgumentNullException">userMessageBox cannot be null.</exception>
        public PersistApplicationStateAsXaml([NotNull] IUserMessageBox userMessageBox)
        {
            if (userMessageBox == null)
            {
                throw new ArgumentNullException("userMessageBox");
            }

            this.userMessageBox = userMessageBox;
        }

        /// <summary>
        /// Gets the full name of the file to save the data into.
        /// The file will be overwritten.
        /// By default this will save to the application folder with the name BudgetAnalyserAppState.xml.
        /// </summary>
        protected virtual string FullFileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.doNotUseFullFileName))
                {
                    string location = Path.GetDirectoryName(GetType().Assembly.Location);
                    this.doNotUseFullFileName = Path.Combine(location, FileName);
                }

                return this.doNotUseFullFileName;
            }
        }

        /// <summary>
        /// Load the user state from the Xaml file on the local disk.
        /// </summary>
        /// <returns>
        /// An array of data objects that are self identifying. This array will need to be processed or broadcasted to the components that consume this data.
        /// </returns>
        /// <exception cref="BadApplicationStateFileFormatException">This will be thrown if the file is invalid.</exception>
        public IEnumerable<IPersistent> Load()
        {
            if (!File.Exists(FullFileName))
            {
                return new List<IPersistent>();
            }

            try
            {
                object serialised = XamlServices.Load(FullFileName);
                    // Will always succeed without exceptions even if bad file format, but will return null.
                var correctFormat = serialised as List<IPersistent>;
                if (correctFormat == null)
                {
                    throw new BadApplicationStateFileFormatException(
                        string.Format(CultureInfo.InvariantCulture,
                            "The file used to store application state ({0}) is not in the correct format. It may have been tampered with.",
                            FullFileName));
                }

                return correctFormat;
            }
            catch (IOException ex)
            {
                this.userMessageBox.Show(ex, ex.Message);
                return new List<IPersistent>();
            }
        }

        /// <summary>
        /// Persist the user data to the Xaml file on the local disk.
        /// </summary>
        /// <param name="modelsToPersist">All components in the App that implement <see cref="IPersistent" /> so the implementation can go get the data to persist.</param>
        public void Persist(IEnumerable<IPersistent> modelsToPersist)
        {
            var data = new List<IPersistent>(modelsToPersist.ToList());
            string serialised = XamlServices.Save(data);
            try
            {
                File.WriteAllText(FullFileName, serialised);
            }
            catch (IOException ex)
            {
                this.userMessageBox.Show(ex, "Unable to save recently used files.");
            }
        }
    }
}