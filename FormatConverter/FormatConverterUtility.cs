using System.Text.RegularExpressions;

namespace FormatConverter
{
    public static class FormatConverterUtility
    {
        // Method to replace sprintf-style format specifiers with std::format-style ones
        public static string ConvertOutputArgToFormat(Match match)
        {
            // Extract the parts of the match
            string beforeOutputArg = match.Groups[1].Value;  // Everything before OutputArg
            string firstArg = match.Groups[3].Success ? match.Groups[3].Value : ""; // First argument (optional)
            string formatString = match.Groups[4].Value;     // Format string (e.g., "TaskMonitor::ExecuteLogFile Script %s aborted")
            string remainingArgs = match.Groups[5].Value;    // Remaining arguments (e.g., input.Filename(), more parameters)

            // Step 1: Replace double %% with a unique placeholder
            formatString = formatString.Replace("%%", "__PERCENT__");

            // Check if there are any format specifiers left in the format string
            bool hasFormatSpecifiers = Regex.IsMatch(formatString, @"%[^%]") && !string.IsNullOrWhiteSpace(remainingArgs);
            // If there are no format specifiers, use Output instead of Format
            if (!hasFormatSpecifiers)
            {
                return $"{beforeOutputArg}Output({firstArg}, \"{formatString.Replace("__PERCENT__", "%")}\");";
            }

            // Step 2: Remove the hh|h|l|ll|z|j|t prefixes
            string intermediateFormatString = Regex.Replace(formatString, @"%([-+ 0#']*\d*(\.\d+)?)(hh|h|l|ll|z|j|t)?([sdixXuofcp])", "%$1$4");

            // Step 3: Handle the width/precision specifiers and translate the format specifiers
            string stdFormatString = Regex.Replace(intermediateFormatString, @"%([-+ 0#']*\d*(\.\d+)?)([sdixXuofcp%])", match2 =>
            {
                string widthPrecision = match2.Groups[1].Value;
                string typeSpecifier = match2.Groups[3].Value;

                // Add colon if width/precision is present or if typeSpecifier is x or X
                if (!string.IsNullOrEmpty(widthPrecision) || typeSpecifier == "x" || typeSpecifier == "X")
                {
                   if (string.IsNullOrEmpty(widthPrecision))
                   {
                      widthPrecision = "#";
                   }
                   widthPrecision = ":" + widthPrecision;
                }

                // Determine the appropriate replacement
                switch (typeSpecifier)
                {
                    case "x": return $"{{{widthPrecision}x}}";
                    case "X": return $"{{{widthPrecision}X}}";
                    case "f": return $"{{{widthPrecision}f}}";
                    case "d":
                    case "s":
                    case "i":
                    case "u":
                        return $"{{{widthPrecision}}}";
                    case "%": return "%"; // Handle single % separately
                    default: return $"{{{widthPrecision}}}";
                }
            });

            // Step 4: Replace the unique placeholder back to %
            stdFormatString = stdFormatString.Replace("__PERCENT__", "%");

            // Handle %.*s pattern for std::string_view
            if (stdFormatString.Contains("%.*s"))
            {
              stdFormatString = Regex.Replace(stdFormatString, @"%(\.\*)s", "{}");
              remainingArgs = Regex.Replace(remainingArgs, @"(\w+)\.size\(\)\s*,\s*(\w+)\.data\(\)\s*,?", "$2, ");
            }

            // Remove all .c_str() in remainingArgs
            remainingArgs = Regex.Replace(remainingArgs, @"\.c_str\(\)", "");

            // Omit firstArg and the comma if firstArg is empty or "QOOutput::OutNormal"
            string formattedCall;
            if (string.IsNullOrEmpty(firstArg) || firstArg == "QOOutput::OutNormal")
            {
                formattedCall = $"{beforeOutputArg}Format(\"{stdFormatString}\", {remainingArgs.TrimEnd(' ', ',')});";
            }
            else
            {
                formattedCall = $"{beforeOutputArg}Format({firstArg}, \"{stdFormatString}\", {remainingArgs.TrimEnd(' ', ',')});";
            }

            return formattedCall;
        }
    }
}
