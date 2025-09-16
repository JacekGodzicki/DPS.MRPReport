using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Utils;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Towary;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator.Models
{
	public class OrderDocumentCreatorModel : ContextBase
    {
        public class ModelCreator
        {
            public Towar Towar { get; set; }
            public Date Term { get; set; }
            public Quantity Quantity { get; set; }
            public Kontrahent Supplier { get; set; }
			public APSReportMRPPosition[] SuggestionPositions { get; set; } = [];
		}

		private readonly DostawcaTowaruUtil _dostawcyTowaruUtil;
		private readonly ConfigReportMRPMainWorker _configReportMRPMainWorker;

		private Date _term;
        private Kontrahent _supplier;
        private Quantity _quantity;

        public OrderDocumentCreatorModel(Context context, ModelCreator creator) : base(context)
        {
            Towar = creator.Towar;
            Term = creator.Term;
            SuggestionQuantity = creator.Quantity;
			SuggestionPositions = creator.SuggestionPositions;
			_quantity = creator.Quantity;
            _supplier = creator.Supplier;
            _dostawcyTowaruUtil = new DostawcaTowaruUtil(context.Session);
			_configReportMRPMainWorker = new ConfigReportMRPMainWorker(context.Session);
			RecalculatePrices();
        }

        public bool ExistsOrder { get; set; }
        public Towar Towar { get; }
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

        public Kontrahent Supplier
        {
            get => _supplier;
            set
            {
                _supplier = value;
                RecalculatePrices();
                OnChanged();
            }
        }

        public Quantity SuggestionQuantity { get; }

        public Quantity Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnChanged();
            }
        }

        public Quantity LogisticalMinimum
        {
            get
            {
                if(GetDostawcaTowaru() is DostawcaTowaru dostawcaTowaru)
                {
                    return dostawcaTowaru.GetAPSExt().LogisticalMinimum;
                }
                return Quantity.Empty;
            }
        }

        public Currency PurchasePriceNetto { get; private set; }
        public Currency PurchasePriceBrutto { get; private set; }

        public object GetListSupplier()
        {
            View view = default;
			if(_configReportMRPMainWorker.OrdersGenLimitContractorsToSuppliers)
            {
			    view = _dostawcyTowaruUtil.GetDostawcyKontrahenciView(Towar);
			}
            else
            {
                view = Session.GetCRM().Kontrahenci.CreateView();
			}
			return new LookupInfo(view);
        }

        private void RecalculatePrices()
        {
            if (Towar is null || Supplier is null)
            {
                PurchasePriceNetto = Currency.Empty;
                PurchasePriceBrutto = Currency.Empty;
                return;
            }

            PurchasePriceNetto = _dostawcyTowaruUtil.GetPurchasePriceNetto(Towar, Supplier);
            PurchasePriceBrutto = _dostawcyTowaruUtil.GetPurchasePriceBrutto(Towar, Supplier);
        }

        private DostawcaTowaru GetDostawcaTowaru()
        {
            if(Towar is null || Supplier is null)
            {
                return null;
            }

            RowCondition rc = new FieldCondition.Equal(nameof(DostawcaTowaru.Towar), Towar);
            rc &= new FieldCondition.Equal(nameof(DostawcaTowaru.Dostawca), Supplier);
            return Towar.Dostawcy[rc].GetFirst();
        }
    }
}
