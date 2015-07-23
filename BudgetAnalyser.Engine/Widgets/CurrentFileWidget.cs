using System;
using System.IO;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Widgets
{
    public class CurrentFileWidget : Widget
    {
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

        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
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
                throw new ArgumentNullException("fileName");
            }

            if (fileName.Length < 30)
            {
                return fileName;
            }

            string proposed = Path.GetFileName(fileName);
            string drive = Path.GetPathRoot(fileName);
            if (proposed.Length < 30)
            {
                return drive + "...\\" + proposed;
            }

            return drive + "...\\" + proposed.Substring(proposed.Length - 30);
        }
    }
}