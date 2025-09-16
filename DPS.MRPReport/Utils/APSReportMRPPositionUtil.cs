using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.Types;

namespace DPS.MRPReport.Utils
{
	public class APSReportMRPPositionUtil
	{
		private readonly Session _session;
		private readonly ConfigReportMRPMainWorker _configReportMRPMainWorker;

		public APSReportMRPPositionUtil(Session session)
		{
			this._session = session;
			_configReportMRPMainWorker = new ConfigReportMRPMainWorker(_session);
		}

		public Date GetDateTakingObtainingPeriod(APSTowarExt apsTowar, Date baseDate)
		{
			if(_configReportMRPMainWorker.IncludeOnlyWorkDays)
			{
				return baseDate.AddBusinessDays(-1 * apsTowar.MRPObtainingPeriod);
			}
			return baseDate.AddDays(-1 * apsTowar.MRPObtainingPeriod);
		}
	}
}
