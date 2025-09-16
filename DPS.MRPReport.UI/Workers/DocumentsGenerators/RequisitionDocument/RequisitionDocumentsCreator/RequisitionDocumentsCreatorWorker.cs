using DPS.MRPReport.Configurations;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Tables;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.Enums;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.Params;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.RequisitionDocument.RequisitionDocumentsCreator;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.UIProviders;
using DPS.MRPReport.UI.Workers.ReportMRP;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Types;
using System;
using System.Linq;

[assembly: SimpleRight(typeof(APSReportMRP), ReportMRPConfiguration.SimpleRights.RequisitionDocumentsCreatorWorker)]
[assembly: Worker(typeof(RequisitionDocumentsCreatorWorker), typeof(ReportMRPUI))]

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.RequisitionDocument.RequisitionDocumentsCreator
{
	public class RequisitionDocumentsCreatorWorker
	{
		[Context]
		public Session Session { get; set; }

		[Context]
		public Context Context { get; set; }

		[Context]
		public ReportMRPUI RaportUI { get; set; }

		[Context]
		public ReportMRPUIParams ReportMRPUIParams { get; set; }

		[Action("Utwórz Zapotrzebowania",
		  Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished
					| ActionMode.Progress | ActionMode.NonCancelProgress,
		  Target = ActionTarget.ToolbarWithText,
		  Priority = 50,
		  Icon = ActionIcon.New)]
		public object Create()
		{
			if(!VerifyReportMRPUIParamsValid(out string message))
			{
				return new MessageBoxInformation("Utwórz Zapotrzebowania", message)
				{
					Type = MessageBoxInformationType.Error,
				};
			}

			return QueryContextInformation.Create((DocumentsCreatorParams pars) =>
			{
				APSReportMRPElement[] elements = GetRaportPozycje(pars);
				if(!VerifyDocumentsCreatorParamsValid(elements, out string message))
				{
					return new MessageBoxInformation("Utwórz Zapotrzebowania", message)
					{
						Type = MessageBoxInformationType.Error,
					};
				}

				Date[] selectedDates = ReportMRPUIParams.GetSelectedDates();
				return new RequisitionDocumentsCreatorUIProvider(Context, selectedDates, elements);
			});
		}

		public static bool IsVisibleCreate(Session session)
		{
			return session.Login.CurrentRole[typeof(APSReportMRPElement), ReportMRPConfiguration.SimpleRights.RequisitionDocumentsCreatorWorker] != AccessRights.Denied;
		}

		private bool VerifyReportMRPUIParamsValid(out string message)
		{
			if(!ReportMRPUIParams.SelectedDates.Any())
			{
				message = "Należy wybrać dni w parametrze \"Wybrane dni\"";
				return false;
			}

			message = string.Empty;
			return true;
		}

		private bool VerifyDocumentsCreatorParamsValid(APSReportMRPElement[] elements, out string message)
		{

			Date[] dates = ReportMRPUIParams.SelectedDates
				.Select(x => x.GetDate())
				.ToArray();

			if(elements is null || !elements.Any(x => IsAnyQuantityDateToCreateZD(x, dates)))
			{
				message = "Dla wybranego towaru i daty brak sugestii dla których można by wygenerować dokument zapotrzebowania.";
				return false;
			}

			ConfigReportMRPMainWorker configReportMRPMainWorker = new ConfigReportMRPMainWorker(Session);
			if(configReportMRPMainWorker.RequisitionDocumentDefs is null)
			{
				message = "W konfiguracji nie wybrano definicji dokumentów zapotrzebowania.";
				return false;
			}

			message = string.Empty;
			return true;
		}

		private bool IsAnyQuantityDateToCreateZD(APSReportMRPElement element, Date[] dates)
		{
			return element.QuantityDates is not null
				&& element.QuantityDates.Any(x => dates.Contains(x.StartDate) && x.Quantity.IsPlus);
		}

		private APSReportMRPElement[] GetRaportPozycje(DocumentsCreatorParams pars)
		{
			if(pars.RowsRange == RowsRangeEnum.Selected)
			{
				return RaportUI.SelectedReportElements;
			}
			return RaportUI.ReportElements;
		}
	}
}
