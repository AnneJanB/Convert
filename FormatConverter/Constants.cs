using System.Text.RegularExpressions;

namespace FormatConverter
{
    public static class Constants
    {
      public const int Cmd_OutputArg = 0x0100;  //must map to ID_OutputArg in .vsct file
      public const int Cmd_AppendArg = 0x0101;  //must map to ID_AppendArg in .vsct file
      public const int Cmd_ExceptionArg = 0x0102;  //must map to ID_AppendArg in .vsct file
 
      public const string OutputArgPattern = @"(.*?)(OutputArg(\s*.*?)\(\s*(?:([^,]*?)\s*,\s*)?""((?s:.*?))""(?:\s*,\s*(.*?))?\s*\);)";
      public const string AppendArgPattern = @"(.*?)(AppendArg(\s*.*?)\(\s*(?:([^,]*?)\s*,\s*)?""((?s:.*?))""(?:\s*,\s*(.*?))?\s*\);)";
      public const string ExceptionArgPattern = @"(.*?)(TExceptionArg(\s*.*?)\(\s*(?:([^,]*?)\s*,\s*)?""((?s:.*?))""(?:\s*,\s*(.*?))?\s*\);)";
    }
}
/*
Explanation of the Pattern
1.	(.*?):
•	This is the first capture group.
•	. matches any character except newline.
•	*? is a non-greedy quantifier, meaning it matches as few characters as possible.
•	This group captures everything before the OutputArg function call.
2.	(OutputArg\(\s*:
•	This matches the literal string OutputArg(.
•	\s* matches zero or more whitespace characters.
•	This part ensures that the pattern starts with OutputArg( followed by optional whitespace.
3.	(?:([^,]*?)\s*,\s*)?:
•	(?: ... ) is a non-capturing group, meaning it groups the enclosed pattern without creating a capture group.
•	([^,]*?) is a capture group that matches any character except a comma, in a non-greedy manner.
•	\s*,\s* matches a comma surrounded by optional whitespace.
•	? makes the entire non-capturing group optional.
•	This part captures the optional first argument of OutputArg, if it exists.
4.	""((?s:.*?))"":
•	"" matches the literal double-quote character.
•	((?s:.*?)) is a capture group that matches the format string.
•	(?s: ... ) enables single-line mode for the enclosed pattern, making . match any character, including newlines.
•	.*? matches any character (including newlines) in a non-greedy manner.
•	This part captures the format string, allowing it to span multiple lines.
5.	(?:\s*,\s*(.*?))?\s*:
•	(?: ... ) is a non-capturing group.
•	\s*,\s* matches a comma surrounded by optional whitespace.
•	(.*?) is a capture group that matches any character in a non-greedy manner.
•	? makes the entire non-capturing group optional.
•	\s* matches zero or more whitespace characters.
•	This part captures the optional remaining arguments of OutputArg, if they exist.
6.	\);:
•	This matches the literal string );.
•	This part ensures that the pattern ends with );.
Summary
•	Capture Group 1: (.*?) captures everything before the OutputArg function call.
•	Capture Group 2: ([^,]*?) captures the optional first argument of OutputArg.
•	Capture Group 3: ((?s:.*?)) captures the format string, allowing it to span multiple lines.
•	Capture Group 4: (.*?) captures the optional remaining arguments of OutputArg.
This regex pattern is designed to match and capture the components of an OutputArg function call, including multiline format strings and optional arguments.
*/