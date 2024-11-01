﻿using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FormatConverter
{
    internal sealed class MyCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("de14b296-430c-4caa-8009-387b7efa157a");

        private readonly AsyncPackage package;

        private MyCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.Execute, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static MyCommand Instance { get; private set; }

        private IAsyncServiceProvider ServiceProvider => this.package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new MyCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE2 dte = ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                return (DTE2)await ServiceProvider.GetServiceAsync(typeof(DTE));
            });

            if (dte == null)
                return;

            // Show dialog to choose between all open documents or all documents in the project
           // var result = MessageBox.Show("Do you want to process all open documents? Click 'No' to process all documents in the project.", "Choose Documents", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

          //  if (result == DialogResult.Cancel)
          //     return;

            bool processOpenDocuments = true; //result == DialogResult.Yes;

            int conversionCount = 0;
            int fileCount = 0;

            if (processOpenDocuments)
            {
                // Loop through all open documents
                foreach (Document doc in dte.Documents)
                {
                    ProcessDocument(doc, ref conversionCount, ref fileCount);
                }
            }
            else
            {
                // Loop through all documents in the project
                foreach (Project project in dte.Solution.Projects)
                {
                    ProcessProjectItems(project.ProjectItems, ref conversionCount, ref fileCount);
                }
            }

            // Show a message box with the number of conversions and files processed
            MessageBox.Show($"{conversionCount} OutputArg instances have been converted in {fileCount} files.", "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ProcessDocument(Document doc, ref int conversionCount, ref int fileCount)
        {
            TextDocument textDoc = doc.Object("TextDocument") as TextDocument;
            if (textDoc != null)
            {
                EditPoint start = textDoc.StartPoint.CreateEditPoint();
                string documentText = start.GetText(textDoc.EndPoint);

                int localConversionCount = 0;

                // Apply regex transformation for OutputArg to Format
                string updatedText = Regex.Replace(documentText, Constants.OutputArgPattern, match =>
                {
                    localConversionCount++;
                    return FormatConverterUtility.ConvertOutputArgToFormat(match);
                }, RegexOptions.Singleline);

                // If the document content has changed, update the document
                if (updatedText != documentText)
                {
                    start.ReplaceText(textDoc.EndPoint, updatedText, (int)vsEPReplaceTextOptions.vsEPReplaceTextKeepMarkers);
                    fileCount++;
                }

                conversionCount += localConversionCount;
            }
        }

        private void ProcessProjectItems(ProjectItems projectItems, ref int conversionCount, ref int fileCount)
        {
            foreach (ProjectItem item in projectItems)
            {
                if (item.Document != null)
                {
                    ProcessDocument(item.Document, ref conversionCount, ref fileCount);
                }

                if (item.ProjectItems != null && item.ProjectItems.Count > 0)
                {
                    ProcessProjectItems(item.ProjectItems, ref conversionCount, ref fileCount);
                }
            }
        }
    }
}
