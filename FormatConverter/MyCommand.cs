using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using stdole;
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

            int conversionCount = 0;
            int fileCount = 0;

            // Loop through all open documents
            foreach (Document doc in dte.Documents)
            {
                TextDocument textDoc = doc.Object("TextDocument") as TextDocument;
                if (textDoc != null)
                {
                    EditPoint start = textDoc.StartPoint.CreateEditPoint();
                    string documentText = start.GetText(textDoc.EndPoint);

                    // Apply regex transformation for OutputArg to Format
                    string updatedText = Regex.Replace(documentText, Constants.OutputArgPattern, match =>
                    {
                        conversionCount++;
                        return FormatConverterUtility.ConvertOutputArgToFormat(match);
                    }, RegexOptions.Singleline);

                    // If the document content has changed, update the document
                    if (updatedText != documentText)
                    {
                        start.ReplaceText(textDoc.EndPoint, updatedText, (int)vsEPReplaceTextOptions.vsEPReplaceTextKeepMarkers);
                        fileCount++;
                    }
                }
            }

            // Show a message box with the number of conversions and files processed
            MessageBox.Show($"{conversionCount} OutputArg instances have been converted in {fileCount} files.", "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
