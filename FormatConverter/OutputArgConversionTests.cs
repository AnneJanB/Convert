using System.Text.RegularExpressions;
using Xunit;

namespace FormatConverter.Tests
{
  public class OutputArgConversionTests
  {
    [Fact]
    public void TestAddition()
    {
      // Arrange
      int number1 = 5;
      int number2 = 10;

      // Act
      int result = number1 + number2;

      // Assert
      Assert.Equal(15, result);
    }
    [Fact]
        public void TestOutputArgConversion()
        {
            // Test cases
            var testCases = new[]
            {
                new
                {
                    Input = "ParmList().OutputArg(Output::OutNormal, \"!! No CCDCity file parameter is defined or cannot open the file\");",
                    Expected = "ParmList().Output(Output::OutNormal, \"!! No CCDCity file parameter is defined or cannot open the file\");"
                },
                new
                {
                    Input = "ParmList().OutputArg(QOOutput::OutNormal, \"%s File %s, warming up\", filename, ParmList().GetFilename(filename));",
                    Expected = "ParmList().Format(\"{} File {}, warming up\", filename, ParmList().GetFilename(filename));"
                },
                new
                {
                    Input = "ParmList().OutputArg(\"Ok %s Done %i records\", filename, recnum);",
                    Expected = "ParmList().Format(\"Ok {} Done {} records\", filename, recnum);"
                },
                new
                {
                    Input ="ParmList().OutputArg(QOOutput::OutNormal,\"%d:%02d:%02d.%03d No of condensed LegsPlus objects = %ld which is %d%% of the original number %ld\". elapse.m_hrs, elapse.m_mins, elapse.m_secs, elapse.m_millies, condense, (int)(100 * condense / legsbeforecondense), legsbeforecondense);",
                    Expected= ""
                }

            };

            foreach (var testCase in testCases)
            {
                string result = Regex.Replace(testCase.Input, Constants.OutputArgPattern, match =>
                {
                    return FormatConverterUtility.ConvertOutputArgToFormat(match);
                }, RegexOptions.Singleline);

                Assert.Equal(testCase.Expected, result);
            }
        }
    }
}
