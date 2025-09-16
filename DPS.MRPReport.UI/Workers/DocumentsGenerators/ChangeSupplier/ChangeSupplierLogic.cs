using DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator.Models;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.UIProviders;
using Soneta.Business;
using Soneta.CRM;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.ChangeSupplier
{
    public class ChangeSupplierLogic
    {
        public readonly Context _context;
        private readonly Kontrahent _newSupplier;

        public ChangeSupplierLogic(Context context, Kontrahent newSupplier)
        {
            _context = context;
			_newSupplier = newSupplier;
        }

        public void Change()
        {
            OrderDocumentCreatorModel[] models = GetOrderDocumentCreatorModels();
			if (!models.Any())
            {
                return;
            }

            foreach (OrderDocumentCreatorModel model in models)
            {
                model.Supplier = _newSupplier;
            }
        }

		private OrderDocumentCreatorModel[] GetOrderDocumentCreatorModels()
		{
			if(_context.Get(out OrderDocumentsCreatorUIProvider orderDocumentsCreatorUIProvider))
			{
				return orderDocumentsCreatorUIProvider.SelectedTowarModels;
			}
			return [];
		}
	}
}
