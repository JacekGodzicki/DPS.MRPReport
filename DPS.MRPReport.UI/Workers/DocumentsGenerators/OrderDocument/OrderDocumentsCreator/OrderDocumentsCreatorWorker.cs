using DPS.MRPReport.Rows;
using DPS.MRPReport.Tables;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.Enums;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator;
using DPS.MRPReport.UI.Workers.ReportMRP;
using DPS.MRPReport.Workers.Config;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.UIProviders;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using DPS.MRPReport.Configurations;

[assembly: SimpleRight(typeof(APSReportMRP), ReportMRPConfiguration.SimpleRights.OrderDocumentsCreatorWorker)]
[assembly: Worker(typeof(OrderDocumentsCreatorWorker), typeof(ReportMRPUI))]

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator
{
    public class OrderDocumentsCreatorWorker
    {
        [Context]
        public Session Session { get; set; }

        [Context]
        public Context Context { get; set; }

        [Context]
        public ReportMRPUI RaportUI { get; set; }

        [Context]
        public ReportMRPUIParams ReportMRPUIParams { get; set; }

        [Action("Utwórz Zamówienia",
          Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished
                    | ActionMode.Progress | ActionMode.NonCancelProgress,
          Target = ActionTarget.ToolbarWithText,
          Priority = 20,
          Icon = ActionIcon.New)]
        public object Create()
        {
            if (!VerifyReportMRPUIParamsValid(out string message))
            {
                return new MessageBoxInformation("Utwórz Zamówienia", message)
                {
                    Type = MessageBoxInformationType.Error,
                };
            }
            return QueryContextInformation.Create((OrderDocumentsCreatorParams pars) =>
            {
				Date[] selectedDates = ReportMRPUIParams.GetSelectedDates();
				APSReportMRPElement[] elements = GetReportMRPElements(pars);
                if(!VerifyOrderDocumentsCreatorParamsValid(elements, selectedDates, out string message))
                {
					return new MessageBoxInformation("Utwórz Zamówienia", message)
					{
						Type = MessageBoxInformationType.Error,
					};
				}

				return new OrderDocumentsCreatorUIProvider(Context, selectedDates, elements);
			});
        }

        public static bool IsVisibleCreate(Session session)
        {
            return session.Login.CurrentRole[typeof(APSReportMRPElement), ReportMRPConfiguration.SimpleRights.OrderDocumentsCreatorWorker] != AccessRights.Denied;
        }

        private bool VerifyReportMRPUIParamsValid(out string message)
        {
            if (!ReportMRPUIParams.SelectedDates.Any())
            {
                message = "Należy wybrać dni w parametrze \"Wybrane dni\"";
                return false;
            }

			message = string.Empty;
			return true;
		}

		private bool VerifyOrderDocumentsCreatorParamsValid(APSReportMRPElement[] elements, Date[] dates, out string message)
        {
			if(elements is null || !elements.Any(x => IsAnyQuantityDateToCreateZD(x, dates)))
			{
				message = "Dla wybranego towaru i daty brak sugestii dla których można by wygenerować dokument ZD.";
				return false;
			}

			ConfigReportMRPMainWorker configReportMRPMainWorker = new ConfigReportMRPMainWorker(Session);
			IEnumerable<APSReportMRPPosition> positionsAll = elements.SelectMany(x => x.Positions);
			if(!configReportMRPMainWorker.AllowGenerateZDFromSP && positionsAll.Any(x => x.IsProductionSuggestion()))
			{
				message = "Wśród wybranych pozycji znajdują się sugestie produkcji (SP)." +
					"W konfiguracji została wyłączona możliwość generowania zamówień do odbiorców (ZD) z sugestii produkcji (SP)";
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

        private APSReportMRPElement[] GetReportMRPElements(OrderDocumentsCreatorParams pars)
        {
            if (pars.RowsRange == RowsRangeEnum.Selected)
            {
                return RaportUI.SelectedReportElements;
            }
            return RaportUI.ReportElements;
        }
    }
}
