using Soneta.Business;
using Soneta.CRM;
using Soneta.Handel;
using System;
using System.Linq;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsGenerator;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.UIProviders;

[assembly: Worker(typeof(OrderDocumentsGeneratorWorker), typeof(OrderDocumentsCreatorUIProvider))]

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsGenerator
{
    public class OrderDocumentsGeneratorWorker
    {
        [Context]
        public Session Session { get; set; }

        [Context]
        public Context Context { get; set; }

        [Context]
        public OrderDocumentsCreatorUIProvider OrderDocumentsCreatorUIProvider { get; set; }

        [Action("Generuj Zamówienie",
          Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished
                    | ActionMode.Progress | ActionMode.NonCancelProgress,
          Target = ActionTarget.ToolbarWithText,
          Priority = 1,
          Icon = ActionIcon.Wizard)]
        public object Action()
        {
            VerifyParams();
            return CreateQueryContextInformation();
        }

        private void VerifyParams()
        {
            if (OrderDocumentsCreatorUIProvider.SelectedTowarModels is null
                || !OrderDocumentsCreatorUIProvider.SelectedTowarModels.Any())
            {
                throw new Exception("Nie wybrano ani jednego towaru!");
            }

            Kontrahent[] suppliers = OrderDocumentsCreatorUIProvider.SelectedTowarModels
                .Select(x => x.Supplier)
                .Distinct()
                .ToArray();

            if (suppliers.Length > 1)
            {
                throw new Exception("Zaznaczone pozycje posiadają wybranych różnych dostawców. Należy wybrać jednego dostawcę!");
            }
        }

        private QueryContextInformation CreateQueryContextInformation()
        {
            OrderDocumentsGeneratorParams pars = new OrderDocumentsGeneratorParams(Context);
            return QueryContextInformation.Create((OrderDocumentsGeneratorParams pars) =>
            {
                return Create(pars);
            });
        }

        private DokumentHandlowy Create(OrderDocumentsGeneratorParams pars)
        {
			DocumentsGeneratorLogic.Params logicPars = new DocumentsGeneratorLogic.Params
            {
                DefDokHandlowego = pars.DefDokHandlowego,
                Magazyn = pars.Magazyn,
                Kontrahent = OrderDocumentsCreatorUIProvider.SelectedTowarModels?.FirstOrDefault()?.Supplier,
                Models = OrderDocumentsCreatorUIProvider.SelectedTowarModels,
                RodzajGrupowania = pars.GroupingType
            };
            return new DocumentsGeneratorLogic(Session, logicPars).Create();
        }
    }
}
