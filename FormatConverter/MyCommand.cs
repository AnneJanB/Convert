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
    public const int CommandId = 0x0100;
    public static readonly Guid CommandSet = new Guid("de14b296-430c-4caa-8009-387b7efa157a");
    // Compile the regex pattern for better performance
    private static readonly Regex OutputArgRegex = new Regex(Constants.OutputArgPattern, RegexOptions.Compiled | RegexOptions.Singleline);

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

        // Apply regex transformation for OutputArg to Format using the compiled regex
        /*string updatedText = OutputArgRegex.Replace(documentText, match =>
        {
          localConversionCount++;
          return FormatConverterUtility.ConvertOutputArgToFormat(match);
        });*/
        // Apply regex transformation for OutputArg to Format
        //   string updatedText = Regex.Replace(documentText, Constants.OutputArgPattern, match =>
        //   {
        //     localConversionCount++;
        //     return FormatConverterUtility.ConvertOutputArgToFormat(match);
        //   }, RegexOptions.Singleline);

        // Find all matches
        /*  StringBuilder updatedTextBuilder = new StringBuilder();
             int lastIndex = 0;

              // Process the document in chunks to avoid processing the entire document at once
             const int chunkSize = 4096; // Adjust chunk size as needed
             int currentIndex = 0;

             while (currentIndex < documentText.Length)
             {
                 int length = Math.Min(chunkSize, documentText.Length - currentIndex);
                 string chunk = documentText.Substring(currentIndex, length);

                 // Use Regex.Match in a loop to find and replace matches incrementally within the chunk
                 Match match = OutputArgRegex.Match(chunk);
                 while (match.Success)
                 {
                     // Append text before the match
                     updatedTextBuilder.Append(chunk, lastIndex, match.Index - lastIndex);

                     // Append the replacement
                     string replacement = FormatConverterUtility.ConvertOutputArgToFormat(match);
                     updatedTextBuilder.Append(replacement);

                     localConversionCount++;
                     lastIndex = match.Index + match.Length;

                     // Find the next match
                     match = OutputArgRegex.Match(chunk, lastIndex);
                 }

                 // Append the remaining text after the last match in the chunk
                 updatedTextBuilder.Append(chunk, lastIndex, chunk.Length - lastIndex);

                 currentIndex += chunkSize;
                 lastIndex = 0; // Reset lastIndex for the next chunk
             }

             string updatedText = updatedTextBuilder.ToString();
     */
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
