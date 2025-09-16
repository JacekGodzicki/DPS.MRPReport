using DPS.MRPReport.Enums;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.ShowSuppliers.Models
{
    public class KontrahentSupplierModel : ContextBase
    {
        private bool _createOrUpdate;

        public KontrahentSupplierModel(Context context) : base(context)
        {
        }

        public double Priority { get; set; }
        public Kontrahent Kontrahent { get; set; }

        public int Term { get; set; }
        public QualityAssessmentEnum QualityAssessment { get; set; }
        public DoubleCy PurchasePriceNetto { get; set; }
        public DoubleCy PurchasePriceBrutto { get; set; }
        public bool IsKontrahentSupplier { get; set; }

        public bool CreateOrUpdate
        {
            get => _createOrUpdate;
            set
            {
                _createOrUpdate = value;
                OnChanged();
            }
        }

        public bool IsReadOnlyCreate() => IsKontrahentSupplier;

        public bool IsReadOnlyUpdate() => !IsKontrahentSupplier;
    }
}