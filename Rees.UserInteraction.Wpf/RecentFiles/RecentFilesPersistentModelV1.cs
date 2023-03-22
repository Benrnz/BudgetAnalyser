using System;
using System.Collections.Generic;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.RecentFiles
{
    /// <summary>
    ///     A <see cref="IPersistent" /> implementation to store the recently used files list.
    /// </summary>
    public class RecentFilesPersistentModelV1 : IPersistent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RecentFilesPersistentModelV1" /> class.
        /// </summary>
        public RecentFilesPersistentModelV1()
        {
            RecentlyUsedFiles = new Dictionary<string, RecentFileV1>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RecentFilesPersistentModelV1" /> class.
        /// </summary>
        /// <param name="recentlyUsedFiles">The model.</param>
        /// <exception cref="System.ArgumentNullException">model</exception>
        /// <exception cref="System.ArgumentException">
        /// Will be thrown if the <paramref name="recentlyUsedFiles"/> is not castable to Dictionary&lt;string, RecentFileV1&gt;
        /// </exception>
        public RecentFilesPersistentModelV1(Dictionary<string, RecentFileV1> recentlyUsedFiles)
        {
            if (recentlyUsedFiles == null)
            {
                throw new ArgumentNullException("recentlyUsedFiles");
            }

            RecentlyUsedFiles = recentlyUsedFiles;
        }

        /// <summary>
        ///     Gets the sequence number for this implementation. This is used to broadcast more crucial higher priority persistent
        ///     data out first, if any.
        /// </summary>
        public int LoadSequence
        {
            get { return 100; }
        }

        /// <summary>
        /// Gets or sets the model to persist to permentant storage.
        /// </summary>
        public Dictionary<string, RecentFileV1> RecentlyUsedFiles { get; set; }
    }
}