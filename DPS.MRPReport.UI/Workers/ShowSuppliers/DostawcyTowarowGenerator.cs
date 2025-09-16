using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.UI.Workers.ShowSuppliers.Models;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Towary;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.ShowSuppliers
{
	public class DostawcyTowarowGenerator
    {
        private readonly Towar _towar;
        private readonly IEnumerable<KontrahentSupplierModel> _kontrahentSupplierModels;
        private readonly Session _session;

        public DostawcyTowarowGenerator(Towar towar, IEnumerable<KontrahentSupplierModel> kontrahentSupplierModels)
        {
            _towar = towar;
            _kontrahentSupplierModels = kontrahentSupplierModels;
            _session = towar.Session;
        }

        public void GenerateOrUpdate()
        {
            IEnumerable<KontrahentSupplierModel> existedSupplierModels = GetExistedSupplierModels();
            UpdateExistedSuppliers(existedSupplierModels);

            IEnumerable<KontrahentSupplierModel> supplierModelsToCreate = GetSupplierModelsToCreate();
            CreateSuppliers(supplierModelsToCreate);
        }

        private IEnumerable<KontrahentSupplierModel> GetExistedSupplierModels()
        {
            return _kontrahentSupplierModels.Where(x => x.IsKontrahentSupplier && x.CreateOrUpdate);
        }

        private void UpdateExistedSuppliers(IEnumerable<KontrahentSupplierModel> kontrahentSupplierModels)
        {
            foreach (KontrahentSupplierModel model in kontrahentSupplierModels)
            {
                DostawcaTowaru dostawcaTowaru = GetDostawcaTowaru(model.Kontrahent);
                UpdateDostawcaTowaru(dostawcaTowaru, model);
            }
        }

        private IEnumerable<KontrahentSupplierModel> GetSupplierModelsToCreate()
        {
            return _kontrahentSupplierModels.Where(x => !x.IsKontrahentSupplier && x.CreateOrUpdate);
        }

        private void CreateSuppliers(IEnumerable<KontrahentSupplierModel> kontrahentSupplierModels)
        {
            foreach (KontrahentSupplierModel model in kontrahentSupplierModels)
            {
                CreateDostawcaTowaru(model);
            }
        }

        private DostawcaTowaru GetDostawcaTowaru(Kontrahent kontrahent)
        {
            RowCondition rc = new FieldCondition.Equal(nameof(DostawcaTowaru.Towar), _towar);
            rc &= new FieldCondition.Equal(nameof(DostawcaTowaru.Dostawca), kontrahent);
            DostawcaTowaru dostawcaTowaru = _session.GetTowary().DostawcyTowaru.PrimaryKey[rc].GetFirst() as DostawcaTowaru;
            if (dostawcaTowaru != null)
            {
                return dostawcaTowaru;
            }
            return _session.GetTowary().DostawcyTowaru.Rows.Changed
                .Cast<DostawcaTowaru>()
                .FirstOrDefault(x => x.Towar == _towar && x.Dostawca == kontrahent);
        }

        private void UpdateDostawcaTowaru(DostawcaTowaru dostawcaTowaru, KontrahentSupplierModel model)
        {
            APSDostawcaTowaruExt dostawcaTowaruExt = dostawcaTowaru.GetAPSExt();
            using (ITransaction transaction = _session.Logout(true))
            {
                dostawcaTowaru.CzasDostawy = model.Term;
                dostawcaTowaruExt.QualityAssessment = model.QualityAssessment;

                transaction.CommitUI();
            }
        }

        private void CreateDostawcaTowaru(KontrahentSupplierModel model)
        {
            using (ITransaction transaction = _session.Logout(true))
            {
                DostawcaTowaru dostawcaTowaru = new DostawcaTowaru();
                _session.AddRow(dostawcaTowaru);
                dostawcaTowaru.Towar = _towar;
                dostawcaTowaru.Dostawca = model.Kontrahent;
                dostawcaTowaru.CzasDostawy = model.Term;

                APSDostawcaTowaruExt dostawcaTowaruExt = dostawcaTowaru.GetAPSExt();
                dostawcaTowaruExt.QualityAssessment = model.QualityAssessment;

                transaction.CommitUI();
            }
        }
    }
}