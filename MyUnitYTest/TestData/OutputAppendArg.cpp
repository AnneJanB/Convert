formatter.FormatAppend("{:-.2f}", machinecpuload); //without percentage at the end
formatter.FormatAppend("{:02}:{:02}:{:02}.{:03}", systemtime.wHour, systemtime.wMinute, systemtime.wSecond, systemtime.wMilliseconds);
formatter.FormatAppend("{} Seconds", *Values().begin());
FormatAppend("{}", value); //later something better
FormatAppend("{f}", value);
formatter.FormatAppend("{:04x}", GetNativeHandle()); //pad with zero's when less than 4 digits
formatter.FormatAppend("{}", value);
formatter.FormatAppend("{_FIX%}.*f", decimals, m_totaltaxvalue);