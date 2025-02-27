using System.Text.RegularExpressions;

namespace FormatConverter.Tests
{
  public class ExceptionArgConversionTests
  {
    [Fact]
    public void TestTExceptionArgConversionFIle()
    {
      // Read test cases from files
      var inputLines = File.ReadAllLines("../../../TestData/InputExceptionArg.cpp");
      var expectedLines = File.ReadAllLines("../../../TestData/OutputExceptionArg.cpp");

      Assert.Equal(inputLines.Length, expectedLines.Length);

      for (int i = 0; i < inputLines.Length; i++)
      {
        string input = inputLines[i];
        string expected = expectedLines[i];

        string result = Regex.Replace(input, Constants.ExceptionArgPattern, match =>
        {
          return FormatConverterUtility.ConvertToFormat(match, Constants.Cmd_ExceptionArg);
        }, RegexOptions.Singleline);

        Assert.Equal(expected, result);
      }
    }
  }
}