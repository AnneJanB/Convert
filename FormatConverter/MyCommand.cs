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
    public static readonly Guid CommandSet = new Guid("de14b296-430c-4caa-8009-387b7efa157a");

    private readonly AsyncPackage package;

    private MyCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));

      if (commandService != null)
      {
        var Cmd_OutputArgID = new CommandID(CommandSet, Constants.Cmd_OutputArg);
        var menuItem = new MenuCommand(this.Execute, Cmd_OutputArgID);
        commandService.AddCommand(menuItem);
        var Cmd_AppendArgID = new CommandID(CommandSet, Constants.Cmd_AppendArg);
        var menuItem2 = new MenuCommand(this.Execute, Cmd_AppendArgID);
        commandService.AddCommand(menuItem2);
        var Cmd_ExceptionArgID = new CommandID(CommandSet, Constants.Cmd_ExceptionArg);
        var menuItem3 = new MenuCommand(this.Execute, Cmd_ExceptionArgID);
        commandService.AddCommand(menuItem3);
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
      var menuCommand = sender as MenuCommand;
      if (menuCommand == null)
      {
        return;
      }

      int commandId = menuCommand.CommandID.ID;

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
          ProcessDocument(doc, ref conversionCount, ref fileCount, commandId);
        }
      }
      else
      {
        // Loop through all documents in the project
        foreach (Project project in dte.Solution.Projects)
        {
          ProcessProjectItems(project.ProjectItems, ref conversionCount, ref fileCount, commandId);
        }
      }

      // Show a message box with the number of conversions and files processed
      if (commandId == Constants.Cmd_OutputArg)
      {
        MessageBox.Show($"{conversionCount} OutputArg instances have been converted in {fileCount} files.", "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else if (commandId == Constants.Cmd_AppendArg)
      {
        MessageBox.Show($"{conversionCount} AppendArg instances have been converted in {fileCount} files.", "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else if (commandId == Constants.Cmd_ExceptionArg)
      {
        MessageBox.Show($"{conversionCount} ExceptionArg instances have been converted in {fileCount} files.", "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

    private void ProcessDocument(Document doc, ref int conversionCount, ref int fileCount, int id)
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
        string pattern = id switch
        {
            Constants.Cmd_OutputArg => Constants.OutputArgPattern,
            Constants.Cmd_AppendArg => Constants.AppendArgPattern,
            Constants.Cmd_ExceptionArg => Constants.ExceptionArgPattern,
            _ => throw new InvalidOperationException("Unknown command ID")
        };

        for (int i = 0; i < chunks.Length; i++)
        {
          string chunk = chunks[i];
          // Add the ';' back to the chunk if it's not the last chunk
          if (i < chunks.Length - 1)
          {
            chunk += ";";
          }

          // Use Regex.Replace with Pattern to find and replace matches within the chunk
          string updatedChunk = Regex.Replace(chunk, pattern, match =>
          {
            localConversionCount++;
            return FormatConverterUtility.ConvertToFormat(match, id);
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
    private void ProcessProjectItems(ProjectItems projectItems, ref int conversionCount, ref int fileCount, int id)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      foreach (ProjectItem item in projectItems)
      {
        if (item.Document != null)
        {
          ProcessDocument(item.Document, ref conversionCount, ref fileCount, id);
        }

        if (item.ProjectItems != null && item.ProjectItems.Count > 0)
        {
          ProcessProjectItems(item.ProjectItems, ref conversionCount, ref fileCount, id);
        }
      }
    }
  }
}