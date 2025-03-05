using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FormatConverter
{
  public static class FormatConverterUtility
  {
    // Method to replace sprintf-style format specifiers with std::format-style ones
    public static string ConvertToFormat(Match match, int id)
    {
      // Extract the parts of the match
      string beforeCmdArg = match.Groups[1].Value;  // Everything before OutputArg
      string afterCmdArg = match.Groups[3].Success ? match.Groups[3].Value : ""; // First argument (optional)
      string firstArg = match.Groups[4].Success ? match.Groups[4].Value : ""; // First argument (optional)
      string formatString = match.Groups[5].Value;     // Format string (e.g., "TaskMonitor::ExecuteLogFile Script %s aborted")
      string remainingArgs = match.Groups[6].Value;    // Remaining arguments (e.g., input.Filename(), more parameters)

      // Step 1: Replace double %% with a unique placeholder
      formatString = formatString.Replace("%%", "__PERCENT__");

      // Check if there are any format specifiers left in the format string
      bool hasFormatSpecifiers = (Regex.IsMatch(formatString, @"%[^%]") || !string.IsNullOrWhiteSpace(remainingArgs));
      if (!hasFormatSpecifiers)
      {
        // If there are no format specifiers, use alternative non Format-style function
        string no_args_replace = id switch
        {
          Constants.Cmd_OutputArg => "Output",
          Constants.Cmd_AppendArg => "Append",
          Constants.Cmd_ExceptionArg => "TException",
          _ => throw new InvalidOperationException("Unknown command ID")
        };
        if (string.IsNullOrEmpty(firstArg))
        {
          return $"{beforeCmdArg}{no_args_replace}{afterCmdArg}(\"{formatString.Replace("__PERCENT__", "%")}\");";
        }
        return $"{beforeCmdArg}{no_args_replace}{afterCmdArg}({firstArg}, \"{formatString.Replace("__PERCENT__", "%")}\");";
      }

      // Step 2: Remove the hh|h|l|ll|z|j|t prefixes
      string intermediateFormatString = Regex.Replace(formatString, @"%([-+ 0#']*\d*(\.\d+)?)(hh|h|l|ll|z|j|t)?([sdixXuofcp])", "%$1$4");

      // Step 3: Handle the width/precision specifiers and translate the format specifiers
      string stdFormatString = Regex.Replace(intermediateFormatString, @"%([-+ 0#']*\d*(\.\d+)?)([sdixXuofcp%])", match2 =>
      {
        string widthPrecision = match2.Groups[1].Value;
        string typeSpecifier = match2.Groups[3].Value;

        // Add colon if width/precision is present or if typeSpecifier is x or X
        if (typeSpecifier == "%")
        {
          return typeSpecifier; // Handle single % separately
        }

        typeSpecifier = typeSpecifier switch
        {
          "p" => "x",
          "P" => "X",
          _ => typeSpecifier
        };
      
        if (!string.IsNullOrEmpty(widthPrecision) || typeSpecifier == "x" || typeSpecifier == "X")
        {
          typeSpecifier = typeSpecifier switch //ommit defaults
          {
            "d" => "",
            "s" => "",
            "u" => "",
            "i" => "",
            _ => typeSpecifier
          };
          widthPrecision = ":" + widthPrecision + typeSpecifier;
        }
        return $"{{{widthPrecision}}}";
      });

  
      // Handle %.*s pattern for std::string_view
      if (stdFormatString.Contains("%.*s"))
      {
        string originalRemainingArgs = remainingArgs;
        remainingArgs = Regex.Replace(remainingArgs, @"(\w+)\.size\(\)\s*,\s*(\w+)\.data\(\)\s*,?", "$2, ");
        if (remainingArgs != originalRemainingArgs)
        {
          stdFormatString = Regex.Replace(stdFormatString, @"%(\.\*)s", "{}");
        }
        else
        {
          stdFormatString = Regex.Replace(stdFormatString, @"%(\.\*)s", "{_FIX.*s:.{}}");
        }
      }

      if (Regex.IsMatch(stdFormatString, @"%[^%]"))  //still a % sign
      {
        stdFormatString = stdFormatString.Replace("%", "{_FIX%}");
      }
      // Step 4: Replace the unique placeholder back to %
      stdFormatString = stdFormatString.Replace("__PERCENT__", "%");
         
      // Remove all .c_str() in remainingArgs
      remainingArgs = Regex.Replace(remainingArgs, @"\.c_str\(\)", "");

      string replace_value = id switch
        {
            Constants.Cmd_OutputArg => "Format",
            Constants.Cmd_AppendArg => "FormatAppend",
            Constants.Cmd_ExceptionArg => "TExceptionFormat",
            _ => throw new InvalidOperationException("Unknown command ID")
        };
  
      string formattedCall;
      // Omit firstArg and the comma if firstArg is empty or "QOOutput::OutNormal"
      
      if (string.IsNullOrEmpty(firstArg) || firstArg == "QOOutput::OutNormal")
      {
        formattedCall = $"{beforeCmdArg}{replace_value}{afterCmdArg}(\"{stdFormatString}\", {remainingArgs.TrimEnd(' ', ',')});";
      }
      else
      {
        formattedCall = $"{beforeCmdArg}{replace_value}{afterCmdArg}({firstArg}, \"{stdFormatString}\", {remainingArgs.TrimEnd(' ', ',')});";
      }

      return formattedCall;
    }
  }
}
