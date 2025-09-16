using Soneta.Business;
using Soneta.Types;
using Soneta.Business.UI;
using DPS.MRPReport.UI.Workers.ReportMRPGenerator;
using DPS.MRPReport.UI.Workers.ReportMRP;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Configurations;

[assembly: SimpleRight(typeof(APSReportMRPElement), ReportMRPConfiguration.SimpleRights.ReportMRPGeneratorWorker)]
[assembly: Worker(typeof(ReportMRPGeneratorWorker), typeof(ReportMRPUI))]

namespace DPS.MRPReport.UI.Workers.ReportMRPGenerator
{
	public class ReportMRPGeneratorWorker
    {
        [Context]
        public Session Session { get; set; }

        [Action("Generuj raport MRP",
            Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished
				| ActionMode.Progress | ActionMode.NonCancelProgress,
			Target = ActionTarget.ToolbarWithText,
            Priority = 1,
            Icon = ActionIcon.Start)]
        public object Generate()
        {
            string caption = "Generuj raport MRP";
            string message = "Czy na pewno chcesz przeliczyć raport MRP?";

            return new MessageBoxInformation(caption, message)
            {
                Type = MessageBoxInformationType.Warning,
                YesHandler = () =>
                {
                    new ReportMRPGeneratorLogic(Session, Date.Today).Generate();
					return "Generowanie zakończone pomyślnie";
                }
            };
        }

        public static bool IsVisibleGenerate(Session session)
        {
            return session.Login.CurrentRole[typeof(APSReportMRPElement), ReportMRPConfiguration.SimpleRights.ReportMRPGeneratorWorker] != AccessRights.Denied;
        }
	}
}