using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using JetBrains.Annotations;
using Rees.Wpf.ApplicationState;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.RecentFiles
{
    /// <summary>
    ///     An implementation of <see cref="IRecentFileManager" /> that persists recently used files using the Application
    ///     State persistence mechanism.
    ///     This class expects to receive a message from the <see cref="IMessenger" /> with a
    ///     <see cref="ApplicationStateRequestedMessage" /> message. To
    ///     this message this class will add any state it needs to save.  It also expects to receive a
    ///     <see cref="ApplicationStateLoadedMessage" /> from
    ///     the <see cref="IMessenger" />, the recently used file list will be extracted from this message at start up.
    /// </summary>
    public class AppStateRecentFileManager : IRecentFileManager
    {
        private Dictionary<string, RecentFileV1> files = new Dictionary<string, RecentFileV1>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="AppStateRecentFileManager" /> class.
        /// </summary>
        /// <param name="messenger">The MvvmLight messenger.</param>
        /// <exception cref="System.ArgumentNullException">messenger cannot be null.</exception>
        public AppStateRecentFileManager([NotNull] IMessenger messenger)
        {
            if (messenger == null)
            {
                throw new ArgumentNullException("messenger");
            }

            messenger.Register<ApplicationStateRequestedMessage>(this, OnPersistentDataRequested);
            messenger.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
        }

        /// <summary>
        ///     Occurs when the <see cref="ApplicationStateLoadedMessage" /> is receieved and the state data has been restored into
        ///     the
        ///     internal model.
        /// </summary>
        public event EventHandler StateDataRestored;

        /// <summary>
        ///     Adds a file to the recently used list.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <returns>
        ///     The full and updated list of all recently used files.
        /// </returns>
        public IEnumerable<KeyValuePair<string, string>> AddFile(string fullFileName)
        {
            if (this.files.ContainsKey(fullFileName))
            {
                this.files[fullFileName].When = DateTime.Now;
            }
            else
            {
                var newFile = new RecentFileV1
                {
                    FullFileName = fullFileName,
                    Name = GetName(fullFileName),
                    When = DateTime.Now
                };
                this.files.Add(fullFileName, newFile);
            }

            return ConvertAndReturnRecentFiles();
        }

        /// <summary>
        ///     All the files in the current list.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Files()
        {
            return ConvertAndReturnRecentFiles();
        }

        /// <summary>
        ///     Converts the internal recently used file list into a Dto object ready for persistence.
        /// </summary>
        public IPersistent GetPersistentData()
        {
            return new RecentFilesPersistentModelV1(this.files);
        }

        /// <summary>
        ///     Removes the specified file from the list.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <returns>
        ///     The full and updated list of all recently used files.
        /// </returns>
        public IEnumerable<KeyValuePair<string, string>> Remove(string fullFileName)
        {
            if (!this.files.ContainsKey(fullFileName))
            {
                return ConvertAndReturnRecentFiles();
            }

            this.files.Remove(fullFileName);
            return ConvertAndReturnRecentFiles();
        }

        /// <summary>
        ///     Updates a file in the list with a date stamp of now.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <returns>
        ///     The full and updated list of all recently used files.
        /// </returns>
        public IEnumerable<KeyValuePair<string, string>> UpdateFile(string fullFileName)
        {
            if (!this.files.ContainsKey(fullFileName))
            {
                return ConvertAndReturnRecentFiles();
            }

            this.files[fullFileName].When = DateTime.Now;
            return ConvertAndReturnRecentFiles();
        }

        /// <summary>
        ///     Gets a friendly name for the file.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        protected virtual string GetName(string fullFileName)
        {
            return Path.GetFileName(fullFileName);
        }

        private IEnumerable<KeyValuePair<string, string>> ConvertAndReturnRecentFiles()
        {
            List<KeyValuePair<string, string>> results = this.files
                .OrderByDescending(f => f.Value.When)
                .Select(f => new KeyValuePair<string, string>(f.Key, f.Value.Name))
                .ToList();
            return results;
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            var recentFilesState = message.ElementOfType<RecentFilesPersistentModelV1>();
            if (recentFilesState != null)
            {
                this.files = recentFilesState.RecentlyUsedFiles;
                var handler = StateDataRestored;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        private void OnPersistentDataRequested(ApplicationStateRequestedMessage message)
        {
            message.PersistThisModel(GetPersistentData());
        }
    }
}