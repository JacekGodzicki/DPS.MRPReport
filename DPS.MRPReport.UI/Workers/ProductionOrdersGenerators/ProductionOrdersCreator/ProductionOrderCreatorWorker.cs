using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Towary;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using DPS.MRPReport.UI.Workers.ReportMRP;
using DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersCreator;
using DPS.MRPReport.Workers.Config;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Utils;
using DPS.MRPReport.Configurations;

[assembly: SimpleRight(typeof(APSReportMRPElement), ReportMRPConfiguration.SimpleRights.ProductionOrderCreatorWorker)]
[assembly: Worker(typeof(ZleceniaProdukcyjneCreatorWorker), typeof(ReportMRPUI))]

namespace DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersCreator
{
	public class ZleceniaProdukcyjneCreatorWorker
	{
		[Context]
		public Session Session { get; set; }

		[Context]
		public Context Context { get; set; }

		[Context]
		public ReportMRPUI ReportMRPUI { get; set; }

		[Context]
		public ReportMRPUIParams ReportMRPUIParams { get; set; }

		[Action("Utwórz Zlecenia produkcyjne",
		  Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished
				  | ActionMode.Progress | ActionMode.NonCancelProgress,
		  Target = ActionTarget.ToolbarWithText,
		  Priority = 30,
		  Icon = ActionIcon.New)]
		public object Generate()
		{
			if(!VerifyReportMRPUIParamsValid(out string message))
			{
				return new MessageBoxInformation("Utwórz Zlecenia produkcyjne", message)
				{
					Type = MessageBoxInformationType.Error,
				};
			}

			return QueryContextInformation.Create((ProductionOrderCreatorParams pars) =>
			{
				Date[] selectedDates = ReportMRPUIParams.GetSelectedDates();
				APSReportMRPElement[] elements = GetReportMRPElements(pars);
				if(!VerifyProductionOrderCreatorParamsValid(elements, selectedDates, out string message))
				{
					return new MessageBoxInformation("Utwórz Zlecenia produkcyjne", message)
					{
						Type = MessageBoxInformationType.Error,
					};
				}

				return new ProductionOrderCreatorUIProvider(Context, selectedDates, elements);
			});
		}

		public static bool IsVisibleGenerate(Session session)
		{
			return session.Login.CurrentRole[typeof(APSReportMRPElement), ReportMRPConfiguration.SimpleRights.ProductionOrderCreatorWorker] != AccessRights.Denied;
		}

		private bool VerifyReportMRPUIParamsValid(out string message)
		{
			if(ReportMRPUIParams.SelectedDates is null || !ReportMRPUIParams.SelectedDates.Any())
			{
				message = "Należy wybrać dni w parametrze \"Wybrane dni\"";
				return false;
			}

			message = string.Empty;
			return true;
		}

		private bool VerifyProductionOrderCreatorParamsValid(APSReportMRPElement[] elements, Date[] dates, out string message)
		{
			if(!ReportMRPUIParams.SelectedDates.Any())
			{
				message = "Należy wybrać dni w parametrze \"Wybrane dni\"";
				return false;
			}

			if(elements is null || !elements.Any(x => IsAnyQuantityDateToCreateZP(x, dates)))
			{
				message = "Dla wybranego towaru i daty brak sugestii dla których można by wygenerować zlecenie produkcyjne.";
				return false;
			}

			ConfigReportMRPMainWorker configReportMRPMainWorker = new ConfigReportMRPMainWorker(Session);
			IEnumerable<APSReportMRPPosition> positionsAll = elements.SelectMany(x => x.Positions);
			if(!configReportMRPMainWorker.AllowGenerateZPFromSZ && positionsAll.Any(x => x.IsPurchaseSuggestion()))
			{
				message = "Wśród wybranych pozycji znajdują się sugestie zakupu (SZ)." +
					"W konfiguracji została wyłączona możliwość generowania zleceń produkcyjnych (ZP) z sugestii zakupu (SZ)";
				return false;
			}

			return VerifyReportPositionsDefaultTechnologiaValid(elements, out message);
		}

		private bool VerifyReportPositionsDefaultTechnologiaValid(APSReportMRPElement[] pozycje, out string message)
		{
			ProTechnologiaUtil proTechnologiaUtil = new ProTechnologiaUtil(Session);
			List<Towar> towaryWithoutTechnologia = pozycje
				.Select(x => x.Towar)
				.Distinct()
				.Where(x => !proTechnologiaUtil.HasDefaultTechnologia(x))
				.ToList();

			if(!towaryWithoutTechnologia.Any())
			{
				message = string.Empty;
				return true;
			}

			Log log = new Log("Generuj ZP - Errors", true);
			foreach(Towar towar in towaryWithoutTechnologia)
			{
				log.WriteLine($"Towar \"{towar}\" nie posiada przypisanej technologii domyślnej");
			}

			message = "Nie wszystkie wybrane towary posiadają technologię domyślną. Więcej informacji w logach.";
			return false;
		}

		private bool IsAnyQuantityDateToCreateZP(APSReportMRPElement element, Date[] dates)
		{
			return element.QuantityDates is not null
				&& element.QuantityDates.Any(x => dates.Contains(x.StartDate) && x.Quantity.IsPlus);
		}

		private APSReportMRPElement[] GetReportMRPElements(ProductionOrderCreatorParams pars)
		{
			if(pars.RowsRange == ProductionOrderCreatorParams.RowsRangeEnum.Selected)
			{
				return ReportMRPUI.SelectedReportElements;
			}
			return ReportMRPUI.ReportElements;
		}
	}
}
