using DPS.MRPReport.Models;
using DPS.MRPReport.Rows;
using DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersCreator.Models;
using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.ProdukcjaPro;
using Soneta.Towary;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersCreator
{
	[Caption("Generowanie zleceń produkcyjnych")]
	public class ProductionOrderCreatorUIProvider : ContextBase
	{
		private readonly Date[] _selectedDates;
		private readonly APSReportMRPElement[] _pozycje;
		private TowarModelContextBase[] _towarModels;
		private TowarModelContextBase[] _selectedTowarModels;

		public ProductionOrderCreatorUIProvider(Context context, Date[] selectedDates, APSReportMRPElement[] pozycje) : base(context)
		{
			_selectedDates = selectedDates;
			_pozycje = pozycje;
		}

		public TowarModelContextBase[] TowarModels
		{
			get
			{
				if(_towarModels is null)
				{
					_towarModels = GetTowarModels();
				}
				return _towarModels
					.Where(x => !x.ExistsZP)
					.ToArray();
			}
		}

		public TowarModelContextBase[] SelectedTowarModels
		{
			get => _selectedTowarModels;
			set
			{
				_selectedTowarModels = value;
			}
		}

		private TowarModelContextBase[] GetTowarModels()
		{
			List<TowarModelContextBase> models = new List<TowarModelContextBase>();
			foreach(APSReportMRPElement pozycja in _pozycje)
			{
				List<QuantityDateModel> ilosciPerDatyDodatnie = pozycja.QuantityDates
					.Where(x => _selectedDates.Contains(x.StartDate) && x.Quantity.IsPlus)
					.ToList();

				foreach(QuantityDateModel iloscPerData in ilosciPerDatyDodatnie)
				{
					Date term = iloscPerData.AvailabilityDate;
					if(term < Date.Today)
					{
						term = Date.Today;
					}

					TowarModelContextBase.ModelCreator modelCreator = new TowarModelContextBase.ModelCreator
					{
						Towar = pozycja.Towar,
						Term = term,
						Quantity = iloscPerData.Quantity,
						Technologia = GetTechnologia(pozycja.Towar),
						SuggestionPositions = iloscPerData.SuggestionPositions
					};

					TowarModelContextBase model = new TowarModelContextBase(Context, modelCreator);
					models.Add(model);
				}
			}

			return models.ToArray();
		}

		private ProTechnologia GetTechnologia(Towar towar)
		{
			ProTechnologiaUtil proTechnologiaUtil = new ProTechnologiaUtil(Session);
			return proTechnologiaUtil.GetDefaultTechnologia(towar);
		}
	}
}
