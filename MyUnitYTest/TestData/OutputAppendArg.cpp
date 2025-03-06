 formatter.FormatAppend("{} {}", Value(), currencies[Currency()].Key());
 formatter.FormatAppend("{:.6f} {}", Value(), currencies[Currency()].Key());
 formatter.AppendArg(formatter.FormatString().Line_c_str(), decimals, amount);