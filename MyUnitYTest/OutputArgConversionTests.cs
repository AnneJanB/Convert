using System.Text.RegularExpressions;
using Xunit;

namespace FormatConverter.Tests
{
  public class OutputArgConversionTests
  {
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
                    Input ="ParmList().OutputArg(QOOutput::OutNormal,\"%d:%02d:%02d.%03d No of condensed LegsPlus objects = %ld which is %d%% of the original number %ld\", elapse.m_hrs, elapse.m_mins, elapse.m_secs, elapse.m_millies, condense, (int)(100 * condense / legsbeforecondense), legsbeforecondense);",
                    Expected= "ParmList().Format(\"{}:{:02}:{:02}.{:03} No of condensed LegsPlus objects = {} which is {}% of the original number {}\", elapse.m_hrs, elapse.m_mins, elapse.m_secs, elapse.m_millies, condense, (int)(100 * condense / legsbeforecondense), legsbeforecondense);"
                }

            };

            foreach (var testCase in testCases)
            {
                string result = Regex.Replace(testCase.Input, Constants.OutputArgPattern, match =>
                {
                    return FormatConverterUtility.ConvertToFormat(match, Constants.Cmd_OutputArg);
                }, RegexOptions.Singleline);

                Assert.Equal(testCase.Expected, result);
            }
        }
        [Fact]
        public void TestOutputArgConversionFIle()
        {
            // Read test cases from files
            var inputLines = File.ReadAllLines("../../../TestData/TestInput.txt");
            var expectedLines = File.ReadAllLines("../../../TestData/TestOutput.txt");

            Assert.Equal(inputLines.Length, expectedLines.Length);

            for (int i = 0; i < inputLines.Length; i++)
            {
                string input = inputLines[i];
                string expected = expectedLines[i];

                string result = Regex.Replace(input, Constants.OutputArgPattern, match =>
                {
                    return FormatConverterUtility.ConvertToFormat(match, Constants.Cmd_OutputArg);
                }, RegexOptions.Singleline);

                Assert.Equal(expected, result);
            }
        }
    }
}
