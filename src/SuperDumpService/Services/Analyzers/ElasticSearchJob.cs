﻿using System;
using System.Threading.Tasks;
using SuperDump.Models;
using SuperDumpService.Models;

namespace SuperDumpService.Services.Analyzers {
	public class ElasticSearchJob : AnalyzerJob {
		private readonly BundleRepository bundleRepo;
		private readonly DumpRepository dumpRepo;
		private readonly ElasticSearchService elasticSearch;

		public ElasticSearchJob(BundleRepository bundleRepo, DumpRepository dumpRepo, ElasticSearchService elasticSearch) {
			this.bundleRepo = bundleRepo;
			this.dumpRepo = dumpRepo;
			this.elasticSearch = elasticSearch;
		}

		public override async Task<AnalyzerState> AnalyzeDump(DumpMetainfo dumpInfo, string analysisWorkingDir, AnalyzerState previousState) {
			if (previousState == AnalyzerState.Failed) {
				return previousState;
			}

			BundleMetainfo bundle = bundleRepo.Get(dumpInfo.BundleId);
			try {
				SDResult result = await dumpRepo.GetResultAndThrow(dumpInfo.Id);
				if (result != null) {
					await elasticSearch.PushResultAsync(result, bundle, dumpInfo);
				}
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
			}
			return previousState;
		}
	}
}