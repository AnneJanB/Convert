TExceptionArg exception("AllinFareFinder::QuoteCombination, Error in CalculateServicesFees function");
throw TExceptionArg("soep");
TExceptionArg error("(%#p) wrong heap alloc: %s, free: %s, size: %u, no: %u, thread %x", pointer, mem_block->m_heap->Name(), Name(), mem_block->Size(), mem_block->AllocationNo(), tid);
TExceptionArg error(ex, "Error ExecuteAsync %s", ex.what());
TExceptionArg(ex, "ProRateMatching failed: %s", ex.what()).Report();
throw TExceptionArg("Agreement expected %d parameters, found %d: %s", EParmsCount, args.GetCount(), input);
TExceptionArg("Incompatible QOQ file: 0x%x, expected 0x%x", pnvqoqstamp, EYieldId).Report();
throw TExceptionArg("Agreement expected %d parameters, found %d: %s", EParmsCount, args.GetCount(), input);
TExceptionArg("Incompatible QOQ file: 0x%x, expected 0x%x", qoq_stamp, EOTHId).Report();
throw TExceptionArg("Wrong tax unit tag9 %c", input.m_taxable_unit_tag9);
TExceptionArg(ex, "Exception: %s: ExecuteFile: %s : executing %s", ex.what(), input.Filename(), input.Buf()).Report(Output());