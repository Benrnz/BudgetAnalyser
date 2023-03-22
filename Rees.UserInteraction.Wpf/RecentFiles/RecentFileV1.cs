using System;

namespace Rees.Wpf.RecentFiles
{
    /// <summary>
    /// A Dto class use for serialisation and persistence of a single recently used file.
    /// </summary>
    public class RecentFileV1
    {
        /// <summary>
        /// Gets or sets the full name of the file.
        /// </summary>
        public string FullFileName { get; set; }

        /// <summary>
        /// Gets or sets the user readable friendly name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets when it was last accessed.
        /// </summary>
        public DateTime When { get; set; }
    }
}