using DPS.MRPReport.Rows;
using DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrderUpdater;
using DPS.MRPReport.UI.Workers.ReportMRP;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: Worker(typeof(ProductionOrderUpdaterWorker), typeof(ReportMRPUI))]

namespace DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrderUpdater
{
	public class ProductionOrderUpdaterWorker
	{
		[Context]
		public Session Session { get; set; }

		[Context]
		public ReportMRPUIParams ReportMRPUIParams { get; set; }

		[Context]
		public ReportMRPUI ReportMRPUI { get; set; }

		[Action("Dodaj do istniejącego zlecenia produkcyjnego",
		  Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished
				  | ActionMode.Progress | ActionMode.NonCancelProgress,
		  Target = ActionTarget.ToolbarWithText,
		  Priority = 40,
		  Icon = ActionIcon.Update)]
		public object Update()
		{
			if(!VerifyReportMRPUIParamsValid(out string message))
			{
				return new MessageBoxInformation("Dodaj do istniejącego zlecenia produkcyjnego", message)
				{
					Type = MessageBoxInformationType.Error,
				};
			}

			return QueryContextInformation.Create((ProductionOrderUpdaterParams pars) =>
			{
				UpdateProZlecenie(pars);
				return "Operacja zakończona pomyślnie";
			});
		}

		private void UpdateProZlecenie(ProductionOrderUpdaterParams pars)
		{
			ProductionOrderUpdaterLogic.Params logicPars = new ProductionOrderUpdaterLogic.Params
			{
				Zlecenie = pars.Zlecenie,
				Quantity = pars.Quantity,
				QuantityDateModels = pars.GetQuantityDateModelsFromSuggestions()
			};
			new ProductionOrderUpdaterLogic(logicPars).Update();
		}

		private bool VerifyReportMRPUIParamsValid(out string message)
		{
			if(ReportMRPUIParams.SelectedDates is null || !ReportMRPUIParams.SelectedDates.Any())
			{
				message = "Należy wybrać dni w parametrze \"Wybrane dni\"";
				return false;
			}

			if(ReportMRPUI.SelectedReportElements is null || ReportMRPUI.SelectedReportElements.Length != 1)
			{
				message = "Należy zaznaczyć dokładnie jeden towar z listy";
				return false;
			}

			Date[] dates = ReportMRPUIParams.SelectedDates
				.Select(x => x.GetDate())
				.ToArray();

			if(!ReportMRPUI.SelectedReportElements.Any(x => IsAnyQuantityDateToCreateZP(x, dates)))
			{
				message = "Dla wybranego towaru i daty brak sugestii dla których można by zaktualizować zlecenie produkcyjne.";
				return false;
			}

			ConfigReportMRPMainWorker configReportMRPMainWorker = new ConfigReportMRPMainWorker(Session);
			IEnumerable<APSReportMRPPosition> positionsAll = ReportMRPUI.SelectedReportElements.SelectMany(x => x.Positions);
			if(!configReportMRPMainWorker.AllowGenerateZPFromSZ && positionsAll.Any(x => x.IsPurchaseSuggestion()))
			{
				message = "Wśród wybranych pozycji znajdują się sugestie zakupu (SZ)." +
					"W konfiguracji została wyłączona możliwość generowania zleceń produkcyjnych (ZP) z sugestii zakupu (SZ)";
				return false;
			}

			message = string.Empty;
			return true;
		}

		private bool IsAnyQuantityDateToCreateZP(APSReportMRPElement element, Date[] dates)
		{
			return element.QuantityDates is not null
				&& element.QuantityDates.Any(x => dates.Contains(x.StartDate) && x.Quantity.IsPlus);
		}
	}
}
