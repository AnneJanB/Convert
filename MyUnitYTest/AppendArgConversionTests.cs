using System.Text.RegularExpressions;

namespace FormatConverter.Tests
{
  public class AppendArgConversionTests
  {
    [Fact]
    public void AppendArgTestFIle()
    {
      // Read test cases from files
      var inputLines = File.ReadAllLines("../../../TestData/InputAppendArg.cpp");
      var expectedLines = File.ReadAllLines("../../../TestData/OutputAppendArg.cpp");

      Assert.Equal(inputLines.Length, expectedLines.Length);

      for (int i = 0; i < inputLines.Length; i++)
      {
        string input = inputLines[i];
        string expected = expectedLines[i];

        string result = Regex.Replace(input, Constants.AppendArgPattern, match =>
        {
          return FormatConverterUtility.ConvertToFormat(match, Constants.Cmd_AppendArg);
        }, RegexOptions.Singleline);

        Assert.Equal(expected, result);
      }
    }
  }
}
