using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xaml;
using BudgetAnalyser.Engine.Persistence;
using Rees.Wpf.Contracts;
using BadApplicationStateFileFormatException = Rees.Wpf.ApplicationState.BadApplicationStateFileFormatException;

namespace BudgetAnalyser.ApplicationState
{
    /// <summary>
    ///     An implementation of <see cref="IPersistApplicationState" /> that saves the user meta-data as Xaml to a file on the
    ///     local disk.
    /// </summary>
    public class PersistBaxAppStateAsXaml : IPersistApplicationState
    {
        private const string FileName = "BudgetAnalyserAppState.xml";
        private readonly IUserMessageBox userMessageBox;

        private string doNotUseFullFileName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistBaxAppStateAsXaml" /> class.
        /// </summary>
        /// <param name="userMessageBox">A service to show the user a message box.</param>
        /// <exception cref="System.ArgumentNullException">userMessageBox cannot be null.</exception>
        public PersistBaxAppStateAsXaml([NotNull] IUserMessageBox userMessageBox)
        {
            if (userMessageBox is null)
            {
                throw new ArgumentNullException(nameof(userMessageBox));
            }

            this.userMessageBox = userMessageBox;
        }

        /// <summary>
        ///     Gets the full name of the file to save the data into.
        ///     The file will be overwritten.
        ///     By default this will save to the application folder with the name BudgetAnalyserAppState.xml.
        /// </summary>
        protected virtual string FullFileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.doNotUseFullFileName))
                {
                    var location = Path.GetDirectoryName(GetType().Assembly.Location);
                    Debug.Assert(location is not null);
                    this.doNotUseFullFileName = Path.Combine(location, FileName);
                }

                return this.doNotUseFullFileName;
            }
        }

        /// <summary>
        ///     Load the user state from the Xaml file on the local disk.
        /// </summary>
        /// <returns>
        ///     An array of data objects that are self identifying. This array will need to be processed or broadcasted to the
        ///     components that consume this data.
        /// </returns>
        /// <exception cref="Rees.Wpf.ApplicationState.BadApplicationStateFileFormatException">
        ///     This will be thrown if the file is
        ///     invalid.
        /// </exception>
        public IEnumerable<IPersistentApplicationStateObject> Load()
        {
            if (!File.Exists(FullFileName))
            {
                return new List<IPersistentApplicationStateObject>();
            }

            try
            {
                object serialised = XamlServices.Load(FullFileName);
                // Will throw Xaml Exception if the xml is corrupt, or the xaml types cannot be found and created.
                var correctFormat = serialised as List<IPersistentApplicationStateObject>;
                if (correctFormat is null)
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
                return HandleCorruptFileGracefully(ex);
            }
            catch (XamlException ex)
            {
                return HandleCorruptFileGracefully(ex);
            }
        }

        /// <summary>
        ///     Persist the user data to the Xaml file on the local disk.
        /// </summary>
        /// <param name="modelsToPersist">
        ///     All components in the App that implement <see cref="IPersistentApplicationStateObject" /> so
        ///     the implementation can go get the data to persist.
        /// </param>
        public void Persist(IEnumerable<IPersistentApplicationStateObject> modelsToPersist)
        {
            var data = new List<IPersistentApplicationStateObject>(modelsToPersist.ToList());
            var serialised = XamlServices.Save(data);
            try
            {
                File.WriteAllText(FullFileName, serialised);
            }
            catch (IOException ex)
            {
                this.userMessageBox.Show(ex, "Unable to save recently used files.");
            }
        }

        private IEnumerable<IPersistentApplicationStateObject> HandleCorruptFileGracefully(Exception ex)
        {
            this.userMessageBox.Show(ex, $"Unable to load previously used application preferences. Preferences have been returned to default settings.\n\n{ex.Message}");
            return new List<IPersistentApplicationStateObject>();
        }
    }
}