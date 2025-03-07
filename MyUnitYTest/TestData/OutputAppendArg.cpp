formatter.FormatAppend("{:.1f}", Tps()); 
formatter.FormatAppend("{:.6f} {}", Value(), currencies[Currency()].Key());
formatter.FormatAppend("{:.2f} {}", Value(), currencies[Currency()].Key());
formatter.AppendArg(formatter.FormatString().Line_c_str(), decimals, amount);