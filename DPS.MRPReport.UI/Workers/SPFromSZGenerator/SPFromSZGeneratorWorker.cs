using Soneta.Business;
using System;
using Soneta.Business.UI;
using DPS.MRPReport.UI.Workers.SPFromSZGenerator;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Configurations;
using DPS.MRPReport.Utils;

[assembly: Worker(typeof(SPFromSZGeneratorWorker), typeof(APSReportMRPPosition))]

namespace DPS.MRPReport.UI.Workers.SPFromSZGenerator
{
	public class SPFromSZGeneratorWorker
	{
		[Context]
		public Session Session { get; set; }

		[Context]
		public APSReportMRPPosition ReportMRPPosition { get; set; }

		[Action("Zmień SZ na SP",
			Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished | ActionMode.Progress
				| ActionMode.NonCancelProgress | ActionMode.OnlyTable,
			Target = ActionTarget.ToolbarWithText,
			Priority = 1,
			Icon = ActionIcon.Wizard)]
		public object Generate()
		{
			if(!VerifyReportMRPPositionValid(out string message))
			{
				return new MessageBoxInformation("Zmień SZ na SP", message)
				{
					Type = MessageBoxInformationType.Error
				};
			}

			SPFromSZGeneratorLogic logic = new SPFromSZGeneratorLogic(ReportMRPPosition);
			logic.Generate();
			return null;
		}

		private bool VerifyReportMRPPositionValid(out string message)
		{
			message = string.Empty;
			ProTechnologiaUtil proTechnologiaUtil = new ProTechnologiaUtil(Session);
			if(!proTechnologiaUtil.HasDefaultTechnologia(ReportMRPPosition.ReportMRPElement.Towar))
			{
				message = "Towar dla wskazanej pozycji nie posiada technologii domyślnej!";
				return false;
			}
			return true;
		}

		public bool IsVisibleGenerate()
		{
			return ReportMRPPosition.DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.SZ;
		}
	}
}
