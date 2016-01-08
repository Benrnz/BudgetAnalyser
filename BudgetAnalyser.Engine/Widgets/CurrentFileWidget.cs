using System;
using System.IO;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Monitors the currently loaded Budget Analyser file and shows the file name.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
    public class CurrentFileWidget : Widget
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentFileWidget" /> class.
        /// </summary>
        public CurrentFileWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[] { typeof(ApplicationDatabase) };
            Size = WidgetSize.Medium;
            WidgetStyle = "ModernTileMediumStyle1";
            Clickable = true;
            DetailedText = "Loading...";
            ColourStyleName = WidgetWarningStyle;
            ImageResourceName = "FolderOpenImage";
            Sequence = 20;
        }

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var appDb = input[0] as ApplicationDatabase;
            if (appDb == null || appDb.FileName.IsNothing())
            {
                ColourStyleName = WidgetWarningStyle;
                DetailedText = "Open";
                ToolTip = "Open an existing Budget Analyser File.";
            }
            else
            {
                ColourStyleName = WidgetStandardStyle;
                DetailedText = ShortenFileName(appDb.FileName);
                ToolTip = appDb.FileName;
            }
        }

        private static string ShortenFileName([NotNull] string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (fileName.Length < 30)
            {
                return fileName;
            }

            var proposed = Path.GetFileName(fileName);
            var drive = Path.GetPathRoot(fileName);
            if (proposed.Length < 30)
            {
                return drive + "...\\" + proposed;
            }

            return drive + "...\\" + proposed.Substring(proposed.Length - 30);
        }
    }
}