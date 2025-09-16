using DPS.MRPReport.Models;
using DPS.MRPReport.UI.Workers.ReportMRP;
using Soneta.Business;
using Soneta.ProdukcjaPro;
using Soneta.Tools;
using Soneta.Towary;
using Soneta.Types;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrderUpdater
{
	public class ProductionOrderUpdaterParams : ContextBase
	{
		private ProZlecenie _zlecenie;
		private Quantity _quantity;

		public ProductionOrderUpdaterParams(Context context) : base(context)
		{
			_quantity = CalculateQuantity();
		}

		[Priority(10)]
		[Caption("Zlecenie")]
		[Required]
		public ProZlecenie Zlecenie
		{
			get => _zlecenie;
			set
			{
				_zlecenie = value;
				OnChanged();
			}
		}

		[Priority(20)]
		[Caption("Ilość zlecenia")]
		[DefaultWidth(15)]
		public Quantity QuantityZlecenie
		{
			get
			{
				if(_zlecenie is null)
				{
					return Quantity.Empty;
				}
				return (Quantity)_zlecenie?.Ilosc;
			}
		}

		[Priority(30)]
		[Caption("Ilość")]
		[Required]
		public Quantity Quantity
		{
			get => _quantity;
			set
			{
				_quantity = value;
				OnChanged();
			}
		}

		public object GetListZlecenie()
		{
			ReportMRPUI reportMRPUI = Context[typeof(ReportMRPUI), false] as ReportMRPUI;
			Towar towar = reportMRPUI.SelectedReportElements.FirstOrDefault()?.Towar;

			if(towar is null)
			{
				return null;
			}

			object[] statuses = [ProStanZlecenia.Przygotowanie, ProStanZlecenia.DoRealizacji, ProStanZlecenia.Rozpoczete];
			RowCondition rc = new FieldCondition.In(nameof(ProZlecenie.Stan), statuses);
			rc &= new FieldCondition.In(nameof(ProZlecenie.Towar), towar);

			View view = Session.GetProdukcjaPro().ProZlecenia.PrimaryKey[rc].CreateView();
			view.Sort = $"{nameof(ProZlecenie.DataRozpoczecia)} desc";
			return new LookupInfo(view);
		}

		private Quantity CalculateQuantity()
		{
			Quantity quantity = Quantity.Zero;

			QuantityDateModel[] quantityDateModels = GetQuantityDateModelsFromSuggestions();
			foreach(QuantityDateModel model in quantityDateModels)
			{
				quantity += model.Quantity;
			}
			return quantity;
		}

		public QuantityDateModel[] GetQuantityDateModelsFromSuggestions()
		{
			ReportMRPUI reportMRPUI = Context[typeof(ReportMRPUI), false] as ReportMRPUI;
			ReportMRPUIParams reportMRPUIParams = Context[typeof(ReportMRPUIParams), false] as ReportMRPUIParams;

			Date[] selectedDates = reportMRPUIParams.GetSelectedDates();

			return reportMRPUI.SelectedReportElements
				.SelectMany(x => x.QuantityDates)
				.Where(x => selectedDates.Contains(x.StartDate))
				.ToArray();
		}
	}
}
