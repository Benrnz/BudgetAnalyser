using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rees.Wpf.ApplicationState;
using Rees.Wpf.Contracts;

namespace Rees.Wpf
{
    /// <summary>
    ///     An interface for a class to manage a recently used file list.
    /// </summary>
    public interface IRecentFileManager
    {
        /// <summary>
        ///     Occurs when the <see cref="ApplicationStateLoadedMessage" /> is receieved and the state data has been restored into
        ///     the
        ///     internal model.
        /// </summary>
        event EventHandler StateDataRestored;

        /// <summary>
        ///     Adds a file to the recently used list.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <returns>The full and updated list of all recently used files.</returns>
        IEnumerable<KeyValuePair<string, string>> AddFile(string fullFileName);

        /// <summary>
        ///     All the files in the current list.
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> Files();

        /// <summary>
        ///     Converts the internal recently used file list into a Dto object ready for persistence.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
                         Justification = "Method will do significant work to create and return a object for persistence.")]
        IPersistent GetPersistentData();

        /// <summary>
        ///     Removes the specified file from the list.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <returns>The full and updated list of all recently used files.</returns>
        IEnumerable<KeyValuePair<string, string>> Remove(string fullFileName);

        /// <summary>
        ///     Updates a file in the list with a date stamp of now.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <returns>The full and updated list of all recently used files.</returns>
        IEnumerable<KeyValuePair<string, string>> UpdateFile(string fullFileName);
    }
}
