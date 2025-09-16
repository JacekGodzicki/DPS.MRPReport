using DPS.MRPReport.Rows;
using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.ProdukcjaPro;
using Soneta.Towary;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersCreator.Models
{
	public class TowarModelContextBase : ContextBase
	{
		public class ModelCreator
		{
			public Towar Towar { get; set; }
			public Date Term { get; set; }
			public Quantity Quantity { get; set; }
			public ProTechnologia Technologia { get; set; }
			public APSReportMRPPosition[] SuggestionPositions { get; set; } = [];
		}

		private Quantity _quantity;
		private ProTechnologia _technologia;
		private Date _term;

		public TowarModelContextBase(Context context, ModelCreator creator) : base(context)
		{
			Towar = creator.Towar;
			SuggestionQuantity = creator.Quantity;
			SuggestionPositions = creator.SuggestionPositions;
			_term = creator.Term;
			_quantity = creator.Quantity;
			_technologia = creator.Technologia;
		}

		public bool ExistsZP { get; set; }
		public Towar Towar { get; }
		public Quantity SuggestionQuantity { get; }
		public APSReportMRPPosition[] SuggestionPositions { get; }

		public Date Term
		{
			get => _term;
			set
			{
				_term = value;
				OnChanged();
			}
		}


		public Quantity Quantity
		{
			get => _quantity;
			set
			{
				_quantity = value;
				OnChanged();
			}
		}

		public ProTechnologia Technologia
		{
			get => _technologia;
			set
			{
				_technologia = value;
				OnChanged();
			}
		}

		public object GetListTechnologia()
		{
			ProTechnologiaUtil proTechnologiaUtil = new ProTechnologiaUtil(Session);
			ProTechnologia[] technologie = proTechnologiaUtil.GetTechnologie(Towar);
			return new LookupInfo(technologie) { ComboBox = false };
		}
	}
}
