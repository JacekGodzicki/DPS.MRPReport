using DPS.MRPReport.UI.Workers.ShowSuppliers;
using DPS.MRPReport.UI.Workers.ShowSuppliers.Models;
using Soneta.Business;
using Soneta.Magazyny;
using Soneta.Towary;
using System;

[assembly: Worker(typeof(ShowSuppliersWorker), typeof(Towar))]

namespace DPS.MRPReport.UI.Workers.ShowSuppliers
{
    public class ShowSuppliersWorker
    {
        [Context]
        public Towar Towar { get; set; }

        [Action("Pokaż dostawców",
           Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished | ActionMode.OnlyForm,
           Target = ActionTarget.ToolbarWithText,
           Priority = 1,
           Icon = ActionIcon.Guy)]
        public object Show()
        {
            KontrahentSupplierModel[] kontrahentSupplierModels = GetKontrahentSupplierModels();
            return CreateUIModel(kontrahentSupplierModels);
        }

        private KontrahentSupplierModel[] GetKontrahentSupplierModels()
        {
            KontrahentSupplierModelsProvider kontrahentSupplierModelsProvider = new KontrahentSupplierModelsProvider(Towar);
            return kontrahentSupplierModelsProvider.GetModels();
        }

        private ShowSuppliersUI CreateUIModel(KontrahentSupplierModel[] kontrahentSupplierModels)
        {
            ShowSuppliersUI showSuppliersUIModel = new ShowSuppliersUI(Towar, kontrahentSupplierModels);
            showSuppliersUIModel.OnCommit += (model) => UpdateDostawcyTowaruBasedOnModels(model.KontrahentSupplierModels);
            return showSuppliersUIModel;
        }

        private void UpdateDostawcyTowaruBasedOnModels(KontrahentSupplierModel[] kontrahentSupplierModels)
        {
            DostawcyTowarowGenerator dostawcyTowarowGenerator = new DostawcyTowarowGenerator(Towar, kontrahentSupplierModels);
            dostawcyTowarowGenerator.GenerateOrUpdate();
        }
    }
}