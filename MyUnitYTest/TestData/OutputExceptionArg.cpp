TExceptionFormat(ex, "ProRateMatching failed: {}", ex.what()).Report();
throw TExceptionFormat("Agreement expected {} parameters, found {}: {}", EParmsCount, args.GetCount(), input);
TExceptionFormat("Incompatible QOQ file: 0x{:x}, expected 0x{:x}", pnvqoqstamp, EYieldId).Report();
throw TExceptionFormat("Agreement expected {} parameters, found {}: {}", EParmsCount, args.GetCount(), input);
TExceptionFormat("Incompatible QOQ file: 0x{:x}, expected 0x{:x}", qoq_stamp, EOTHId).Report();
throw TExceptionFormat("Wrong tax unit tag9 {}", input.m_taxable_unit_tag9);
TExceptionArg error(ex, "Error ExecuteAsync %s", ex.what());
TExceptionFormat(ex, "Exception: {}: ExecuteFile: {} : executing {}", ex.what(), input.Filename(), input.Buf()).Report(Output());