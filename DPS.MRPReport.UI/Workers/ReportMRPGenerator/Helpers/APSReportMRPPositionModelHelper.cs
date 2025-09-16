using DPS.MRPReport.Configurations;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.UI.Workers.ReportMRPGenerator.Models;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.ReportMRPGenerator.Helpers
{
	public class APSReportMRPPositionModelHelper
	{
		public Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> GroupAndSortModels(IEnumerable<APSReportMRPPositionModel> models)
		{
			return models
				.GroupBy(x => x.TowarExt)
				.OrderBy(kv => (int)kv.Key.MRPObtainingMethod)
				.ToDictionary(
					kv => kv.Key,
					kv => SortModels(kv)
				);
		}

		public List<APSReportMRPPositionModel> SortModels(IEnumerable<APSReportMRPPositionModel> models)
		{
			return models
				.OrderBy(x => x.AvailabilityDate)
				.ThenByDescending(x => (int)x.Direction)
				.ThenBy(x => x.DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.BO ? 1 : 0)
				.ToList();
		}

	}
}
