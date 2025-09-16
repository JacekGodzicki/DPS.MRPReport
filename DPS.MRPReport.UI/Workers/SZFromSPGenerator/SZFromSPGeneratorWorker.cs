using DPS.MRPReport.Configurations;
using DPS.MRPReport.Rows;
using DPS.MRPReport.UI.Workers.SZFromSPGenerator;
using Soneta.Business;
using System;

[assembly: Worker(typeof(SZFromSPGeneratorWorker), typeof(APSReportMRPPosition))]

namespace DPS.MRPReport.UI.Workers.SZFromSPGenerator
{
	public class SZFromSPGeneratorWorker
	{
		[Context]
		public Session Session { get; set; }

		[Context]
		public APSReportMRPPosition ReportMRPPosition { get; set; }

		[Action("Zmień SP na SZ",
			Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished | ActionMode.Progress 
				| ActionMode.NonCancelProgress | ActionMode.OnlyTable,
			Target = ActionTarget.ToolbarWithText,
			Priority = 1,
			Icon = ActionIcon.Wizard)]
		public void Generate()
		{
			SZFromSPGeneratorLogic logic = new SZFromSPGeneratorLogic(ReportMRPPosition);
			logic.Generate();
		}

		public bool IsVisibleGenerate()
		{
			return ReportMRPPosition.DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.SP;
		}
	}
}
