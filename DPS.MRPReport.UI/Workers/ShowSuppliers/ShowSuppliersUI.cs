using DPS.MRPReport.UI.Workers.ShowSuppliers.Models;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Towary;
using Soneta.Types;
using System;

namespace DPS.MRPReport.UI.Workers.ShowSuppliers
{
    [Caption("Pokaż dostawców")]
    public class ShowSuppliersUI : ICommittable
    {
        private readonly Towar _towar;
        private readonly Session _session;
        private readonly KontrahentSupplierModel[] _kontrahentSupplierModels;

        public ShowSuppliersUI(Towar towar, KontrahentSupplierModel[] kontrahentSupplierModels)
        {
            _towar = towar;
            _kontrahentSupplierModels = kontrahentSupplierModels;
            _session = towar.Session;
        }

        public KontrahentSupplierModel[] KontrahentSupplierModels => _kontrahentSupplierModels;

        public event Action<ShowSuppliersUI> OnCommit;

        public object OnCommitted(Context context)
        {
            return null;
        }

        public object OnCommitting(Context context)
        {
            OnCommit?.Invoke(this);
            return null;
        }
    }
}