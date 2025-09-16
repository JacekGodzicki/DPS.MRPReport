using DPS.MRPReport.Configurations;
using DPS.MRPReport.Models;
using DPS.MRPReport.Rows;
using DPS.MRPReport.UI.Workers.ReportMRPGenerator;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.ProdukcjaPro;
using Soneta.Towary;
using Soneta.Types;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrderUpdater
{
	public class ProductionOrderUpdaterLogic
	{
		public class Params
		{
			public ProZlecenie Zlecenie { get; set; }
			public Quantity Quantity { get; set; }
			public QuantityDateModel[] QuantityDateModels { get; set; }
		}


		private readonly ProZlecenie _zlecenie;
		private readonly Quantity _quantity;
		private readonly Session _session;
		private readonly QuantityDateModel[] _quantityDateModels = [];

		public ProductionOrderUpdaterLogic(Params pars)
		{
			_zlecenie = pars.Zlecenie;
			_quantity = pars.Quantity;
			_quantityDateModels = pars.QuantityDateModels;
			_session = pars.Zlecenie.Session;
		}

		public void Update()
		{
			UpdateZlecenie();
			UpdateSuggestions();
		}

		private void UpdateZlecenie()
		{
			using(ITransaction transaction = _session.Logout(true))
			{
				_zlecenie.Ilosc += (Amount)_quantity;
				transaction.CommitUI();
			}
		}

		private void UpdateSuggestions()
		{
			ConfigReportMRPMainWorker configWorker = new ConfigReportMRPMainWorker(_session);
			if(configWorker.RecalculateAfterCreatingDocumentOrOrder)
			{
				GenerateUpdatedRaportMRP();
			}
			else
			{
				UpdateSuggestionPositions();
			}
		}

		private void GenerateUpdatedRaportMRP()
		{
			Date dateFrom = _session.Global.Features.GetDate(FeatureConstants.FeatureNamesGlobal.ReportMRPDataOstatniePrzeliczenie);
			new ReportMRPGeneratorLogic(_session, dateFrom).Generate();
		}

		private void UpdateSuggestionPositions()
		{
			APSReportMRPPosition[] suggestionPositions = GetSuggestionPositions();

			using(ITransaction transaction = _session.Logout(true))
			{
				foreach(APSReportMRPPosition position in suggestionPositions)
				{
					position.DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.ZP;
					position.DocumentNumber = _zlecenie.Numer.NumerPelny;
					position.CreatedFromSuggestion = true;
					position.RelatedSuggestionParentRel.Delete();
					UpdateRelatedPositions(position);
				}

				transaction.CommitUI();
			}
		}

		private APSReportMRPPosition[] GetSuggestionPositions()
		{
			return _quantityDateModels
				.SelectMany(x => x.SuggestionPositions)
				.Select(x => _session.Get(x))
				.ToArray();
		}

		private void UpdateRelatedPositions(APSReportMRPPosition position)
		{
			foreach(APSReportMRPPosition child in position.Children)
			{
				child.DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.PR;
				child.DocumentNumber = _zlecenie.Numer.NumerPelny;
				child.CreatedFromSuggestion = true;
			}
		}
	}
}
