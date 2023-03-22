using System;
using GalaSoft.MvvmLight.Command;

namespace Rees.Wpf.RecentFiles
{
    /// <summary>
    ///     A <see cref="RelayCommand" /> that is used for a recently used (or opened) file list.
    /// </summary>
    public class RecentFileRelayCommand : RelayCommand<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RecentFileRelayCommand" /> class.
        /// </summary>
        /// <param name="name">The readable user facing name of the file.</param>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <param name="execute">The action to execute.</param>
        public RecentFileRelayCommand(string name, string fullFileName, Action<string> execute) : base(execute)
        {
            Name = name;
            FullFileName = fullFileName;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RecentFileRelayCommand" /> class.
        /// </summary>
        /// <param name="name">The readable user facing name of the file.</param>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">A delegate that determines if the action can execute.</param>
        public RecentFileRelayCommand(string name, string fullFileName, Action<string> execute,
                                      Func<string, bool> canExecute) : base(execute, canExecute)
        {
            Name = name;
            FullFileName = fullFileName;
        }

        /// <summary>
        ///     Gets the full name of the file.
        /// </summary>
        public string FullFileName { get; }

        /// <summary>
        ///     Gets the readable user facing name of the file.
        /// </summary>
        public string Name { get; }
    }
}