using Soneta.Business;
using Soneta.CRM;
using Soneta.Handel;
using Soneta.Towary;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using DPS.MRPReport.UI.Workers.ShowSuppliers.Models;
using DPS.MRPReport.Utils;
using DPS.MRPReport.Enums;
using DPS.MRPReport.Extensions;

namespace DPS.MRPReport.UI.Workers.ShowSuppliers
{
    public class KontrahentSupplierModelsProvider
    {
        private readonly Towar _towar;
        private readonly Session _session;
		private readonly TowarUtil _towarUtils;

        public KontrahentSupplierModelsProvider(Towar towar)
        {
            _towar = towar;
            _session = towar.Session;
            _towarUtils = new TowarUtil(towar);
        }

        public KontrahentSupplierModel[] GetModels()
        {
            Context context = Context.Empty.Clone(_session);

            List<KontrahentSupplierModel> kontrahentSupplierModels = new List<KontrahentSupplierModel>();
            Dictionary<Kontrahent, List<PozycjaDokHandlowego>> positionsPZPerContractors = GetPositionsPZPerContractors();
            foreach (var positionsPZPerContractor in positionsPZPerContractors)
            {
                Kontrahent kontrahent = positionsPZPerContractor.Key;
                List<PozycjaDokHandlowego> positionsPZ = positionsPZPerContractor.Value;

                ICena purchasePrice = _towarUtils.GetLastPurchasePrice(kontrahent);
                KontrahentSupplierModel kontrahentSupplierModel = new KontrahentSupplierModel(context)
                {
                    Kontrahent = kontrahent,
                    Term = CalculateAvgTerm(positionsPZ),
                    PurchasePriceNetto = purchasePrice.Netto,
                    PurchasePriceBrutto = purchasePrice.Brutto,
                    QualityAssessment = QualityAssessmentEnum.Assessment1,
                    IsKontrahentSupplier = kontrahent.IsDostawcaTowaru(_towar)
                };
                kontrahentSupplierModels.Add(kontrahentSupplierModel);
            }
            return kontrahentSupplierModels.ToArray();
        }

        private Dictionary<Kontrahent, List<PozycjaDokHandlowego>> GetPositionsPZPerContractors()
        {
            IEnumerable<PozycjaDokHandlowego> positionsPZ = GetPositionsPZ();
            return positionsPZ
                .GroupBy(x => x.Dokument.Kontrahent)
                .ToDictionary(kv => kv.Key, kv => kv.ToList());
        }

        private IEnumerable<PozycjaDokHandlowego> GetPositionsPZ()
        {
            RowCondition rc = new FieldCondition.Equal($"{nameof(PozycjaDokHandlowego.Dokument)}.{nameof(PozycjaDokHandlowego.Dokument.Kategoria)}", KategoriaHandlowa.PrzyjęcieMagazynowe);
            rc &= new FieldCondition.In($"{nameof(PozycjaDokHandlowego.Dokument)}.{nameof(PozycjaDokHandlowego.Dokument.Stan)}", [StanDokumentuHandlowego.Zatwierdzony, StanDokumentuHandlowego.Zablokowany]);
            rc &= new FieldCondition.Null($"{nameof(PozycjaDokHandlowego.Dokument)}.{nameof(PozycjaDokHandlowego.Dokument.Kontrahent)}", false);
            rc &= new FieldCondition.Equal(nameof(PozycjaDokHandlowego.Towar), _towar);
            rc &= new FieldCondition.NotEqual(nameof(PozycjaDokHandlowego.StatusPozycji), StatusPozycji.Anulowana);
            return _session.GetHandel().PozycjeDokHan.PrimaryKey[rc]
                .Cast<PozycjaDokHandlowego>();
        }

        private int CalculateAvgTerm(List<PozycjaDokHandlowego> positionsPZ)
        {
            double countDocuments = 0;
            double sumDays = 0;

            foreach (PozycjaDokHandlowego positionPZ in positionsPZ)
            {
                IEnumerable<PozycjaDokHandlowego> positionsZK = GetPositionsZK(positionPZ);
                IEnumerable<PozycjaDokHandlowego> positionsZD = GetPositionsZD(positionsZK);
                foreach (PozycjaDokHandlowego positionZD in positionsZD)
                {
                    countDocuments += 1;
                    sumDays += new FromTo(positionZD.Data, positionPZ.Data).Days;
                }
            }

            if (countDocuments == 0)
            {
                return 0;
            }
            return (int)Math.Round(sumDays / countDocuments);
        }

        private IEnumerable<PozycjaDokHandlowego> GetPositionsZK(PozycjaDokHandlowego positionPZ)
        {
            return positionPZ.Nadrzędne
                .Cast<PozycjaDokHandlowego>()
                .Where(x => x.Dokument.IsZatwierdzonyOrZablokowany() && (x.Dokument.Kategoria == KategoriaHandlowa.Zakup || x.Dokument.Kategoria == KategoriaHandlowa.KorektaZakupu))
                .Distinct();
        }

        private IEnumerable<PozycjaDokHandlowego> GetPositionsZD(IEnumerable<PozycjaDokHandlowego> positionsZK)
        {
            return positionsZK
                .SelectMany(x => x.Nadrzędne.Cast<PozycjaDokHandlowego>())
                .Where(x => x.Dokument.IsZatwierdzonyOrZablokowany() && x.Dokument.Kategoria == KategoriaHandlowa.ZamówienieDostawcy)
                .Distinct();
        }
    }
}