using DPS.MRPReport.Configurations;
using DPS.MRPReport.Rows;
using Soneta.Business;

namespace DPS.MRPReport.UI.Workers.SPFromSZGenerator
{
	public class SPFromSZGeneratorLogic
	{
		private readonly APSReportMRPPosition _reportMRPPosition;
		private readonly Session _session;

		public SPFromSZGeneratorLogic(APSReportMRPPosition reportMRPPosition)
		{
			_reportMRPPosition = reportMRPPosition;
			_session = reportMRPPosition.Session;
		}

		public void Generate()
		{
			using(ITransaction transaction = _session.Logout(true))
			{
				_reportMRPPosition.DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.SP;

				transaction.CommitUI();
			}
		}
	}
}
