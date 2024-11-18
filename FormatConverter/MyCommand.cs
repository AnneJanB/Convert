using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
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
    public const int Cmd_OutputArg = 0x0100;
    public const int Cmd_AppendArg = 0x0101;
    public static readonly Guid CommandSet = new Guid("de14b296-430c-4caa-8009-387b7efa157a");
    // Compile the regex pattern for better performance
    private static readonly Regex OutputArgRegex = new Regex(Constants.OutputArgPattern, RegexOptions.Compiled | RegexOptions.Singleline);

    private readonly AsyncPackage package;

    private MyCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));

      if (commandService != null)
      {
        var Cmd_OutputArgID = new CommandID(CommandSet, Cmd_OutputArg);
        var menuItem = new MenuCommand(this.Execute, Cmd_OutputArgID);
        commandService.AddCommand(menuItem);

        // Register another command
        var Cmd_AppendArgID = new CommandID(CommandSet, Cmd_AppendArg);
        var anotherMenuItem = new MenuCommand(this.ExecuteAnotherCommand, Cmd_AppendArgID);
        commandService.AddCommand(anotherMenuItem);
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

    private void ExecuteAnotherCommand(object sender, EventArgs e)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      // Implement the logic for the new command here
      MessageBox.Show("Another command executed!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ProcessDocument(Document doc, ref int conversionCount, ref int fileCount)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      TextDocument textDoc = doc.Object("TextDocument") as TextDocument;
      if (textDoc != null)
      {
        EditPoint start = textDoc.StartPoint.CreateEditPoint();
        string documentText = start.GetText(textDoc.EndPoint);

        int localConversionCount = 0;

        // Split the document into chunks based on the ';' delimiter
        StringBuilder updatedTextBuilder = new StringBuilder();

        // Split the document into chunks based on the ';' delimiter
        string[] chunks = documentText.Split(';');

        for (int i = 0; i < chunks.Length; i++)
        {
          string chunk = chunks[i];
          // Add the ';' back to the chunk if it's not the last chunk
          if (i < chunks.Length - 1)
          {
            chunk += ";";
          }

          // Use Regex.Replace with Constants.OutputArgPattern to find and replace matches within the chunk
          string updatedChunk = Regex.Replace(chunk, Constants.OutputArgPattern, match =>
          {
            localConversionCount++;
            return FormatConverterUtility.ConvertOutputArgToFormat(match);
          }, RegexOptions.Singleline);
          // Append the updated chunk to the StringBuilder
          updatedTextBuilder.Append(updatedChunk);
        }

        string updatedText = updatedTextBuilder.ToString();

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
      ThreadHelper.ThrowIfNotOnUIThread();

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
