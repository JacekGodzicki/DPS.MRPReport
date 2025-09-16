using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Towary;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.RequisitionDocument.RequisitionDocumentsGenerator.Models
{
	public class RequisitionDocumentGeneratorTowarModel : ContextBase
    {
        public class ModelCreator
        {
            public Towar Towar { get; set; }
            public Date Term { get; set; }
            public Quantity Quantity { get; set; }
            public Kontrahent Supplier { get; set; }
        }

        private DostawcaTowaruUtil _dostawcyTowaruUtil;
        private Kontrahent _supplier;
        private Quantity _quantity;

        public RequisitionDocumentGeneratorTowarModel(Context context, ModelCreator creator) : base(context)
        {
            Towar = creator.Towar;
            Term = creator.Term;
            SuggestionQuantity = creator.Quantity;
            _quantity = creator.Quantity;
            _supplier = creator.Supplier;
            _dostawcyTowaruUtil = new DostawcaTowaruUtil(context.Session);
            RecalculatePrices();
        }

        public bool ExistsOrder { get; set; }

        public Towar Towar { get; }

        public Date Term { get; }

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

        public Currency PurchasePriceNetto { get; private set; }
        public Currency PurchasePriceBrutto { get; private set; }

        public object GetListSupplier()
        {
            View<Kontrahent> view = _dostawcyTowaruUtil.GetDostawcyKontrahenciView(Towar);
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
    }
}
