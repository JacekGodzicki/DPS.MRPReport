using DPS.MRPReport.UI.Workers.DocumentsGenerators.ChangeSupplier;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.UIProviders;
using Soneta.Business;
using System;

[assembly: Worker(typeof(ChangeSupplierWorker), typeof(OrderDocumentsCreatorUIProvider))]

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.ChangeSupplier
{
	public class ChangeSupplierWorker
    {
		[Context]
		public Context Context { get; set; }

        [Context]
        public ChangeSupplierParams Pars { get; set; }

        [Action("Zmień zbiorczo dostawcę",
          Mode = ActionMode.SingleSession,
          Target = ActionTarget.ToolbarWithText,
          Priority = 1,
          Icon = ActionIcon.Guy)]
        public void Action()
        {
			new ChangeSupplierLogic(Context, Pars.Supplier).Change();
        }
    }
}
