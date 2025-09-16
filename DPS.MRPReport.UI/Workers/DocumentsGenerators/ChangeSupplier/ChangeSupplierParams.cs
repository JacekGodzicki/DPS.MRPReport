using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Towary;
using Soneta.Types;
using System.Linq;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.UIProviders;
using DPS.MRPReport.Workers.Config;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.ChangeSupplier
{
	public class ChangeSupplierParams : ContextBase
    {
		private readonly ConfigReportMRPMainWorker _configReportMRPMainWorker;
		private readonly DostawcaTowaruUtil _dostawcyTowaruUtil;
        private Kontrahent _supplier;

        public ChangeSupplierParams(Context context) : base(context)
        {
			_configReportMRPMainWorker = new ConfigReportMRPMainWorker(context.Session);
			_dostawcyTowaruUtil = new DostawcaTowaruUtil(context.Session);
			if (context.Get(out OrderDocumentsCreatorUIProvider generateDocumentsZDUI))
            {
                _supplier = generateDocumentsZDUI.SelectedTowarModels?.FirstOrDefault()?.Supplier;
            }
        }

        [Context]
        [Hidden]
        public OrderDocumentsCreatorUIProvider GenerateDocumentsZDUI { get; set; }

        [Caption("Dostawca")]
        [Required]
        public Kontrahent Supplier
        {
            get => _supplier;
            set
            {
                _supplier = value;
                OnChanged();
            }
        }

        public object GetListSupplier()
        {
			View view = default;
			if(_configReportMRPMainWorker.OrdersGenLimitContractorsToSuppliers)
			{
			    Towar[] towary = GenerateDocumentsZDUI.TowarModels
	                .Select(x => x.Towar)
	                .ToArray();

			    view = _dostawcyTowaruUtil.GetDostawcyKontrahenciView(towary);
			}
			else
			{
				view = Session.GetCRM().Kontrahenci.CreateView();
			}
			return new LookupInfo(view);
		}
    }
}
