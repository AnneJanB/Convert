formatter.AppendArg("%.1f", Tps()); 
formatter.AppendArg("%f %s", Value(), currencies[Currency()].Key());
formatter.AppendArg("%.2f %s", Value(), currencies[Currency()].Key());
formatter.AppendArg(formatter.FormatString().Line_c_str(), decimals, amount);
formatter.AppendArg("%x", QOQFormatId());