TExceptionArg(ex, "ProRateMatching failed: %s", ex.what()).Report();
throw TExceptionArg("Agreement expected %d parameters, found %d: %s", EParmsCount, args.GetCount(), input);
TExceptionArg("Incompatible QOQ file: 0x%x, expected 0x%x", pnvqoqstamp, EYieldId).Report();
throw TExceptionArg("Agreement expected %d parameters, found %d: %s", EParmsCount, args.GetCount(), input);
TExceptionArg("Incompatible QOQ file: 0x%x, expected 0x%x", qoq_stamp, EOTHId).Report();
throw TExceptionArg("Wrong tax unit tag9 %c", input.m_taxable_unit_tag9);
TExceptionArg error(ex, "Error ExecuteAsync %s", ex.what());
TExceptionArg(ex, "Exception: %s: ExecuteFile: %s : executing %s", ex.what(), input.Filename(), input.Buf()).Report(Output());