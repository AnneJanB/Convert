namespace FormatConverter
{
    public static class Constants
    {
       public const string OutputArgPattern = @"(.*?)(OutputArg\(\s*(?:([^,]*?)\s*,\s*)?""([^""]*)""\s*(?:,\s*(.*?)\s*)?\)\s*;)";
       //public const string OutputArgPatternOld = @"(.*?)(OutputArg\(\s*(?:(QOOutput::OutNormal|[^,]*?)\s*,\s*)?""([^""]*)""\s*(?:,\s*(.*?)\s*)?\)\s*;)";
      // public const string OutputArgPattern =    @"(.*?)(OutputArg\(\s*(?:(.*?),\s*)?""([^""]*)""\s*(?:,\s*(.*?)\s*)?\)\s*;)";
                                  //               @"(.*?)(OutputArg\(\s*(?:(.*?),\s*)?""([^""]*)""\s*(?:,\s*(.*?)\s*)?\)\s*;)";
    }
}
