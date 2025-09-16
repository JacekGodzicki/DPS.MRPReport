using DPS.MRPReport.Extensions;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Towary;
using Soneta.Types;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Utils
{
    public class DostawcaTowaruUtil
    {
        private readonly Session _session;

        public DostawcaTowaruUtil(Session session)
        {
            _session = session;
        }

        public View<Kontrahent> GetDostawcyKontrahenciView(Towar towar)
        {
            List<Kontrahent> kontrahenci = towar.Dostawcy
                .Cast<DostawcaTowaru>()
                .OrderByDescending(x => x.Domyslny)
                .Select(x => x.Dostawca)
                .ToList();

            return new View<Kontrahent>(_session.GetCRM().Kontrahenci.PrimaryKey, kontrahenci);
        }

        public View<Kontrahent> GetDostawcyKontrahenciView(IEnumerable<Towar> towary)
        {
            RowCondition rc = new FieldCondition.In(nameof(DostawcaTowaru.Towar), towary.ToArray());
            List<Kontrahent> kontrahenci = _session.GetTowary().DostawcyTowaru.PrimaryKey[rc]
                .Cast<DostawcaTowaru>()
                .OrderByDescending(x => x.Domyslny)
                .Select(x => x.Dostawca)
                .Distinct()
                .ToList();

            return new View<Kontrahent>(_session.GetCRM().Kontrahenci.PrimaryKey, kontrahenci);
        }

        public Currency GetPurchasePriceNetto(Towar towar, Kontrahent kontrahent)
        {
            RowCondition rc = new FieldCondition.Equal(nameof(DostawcaTowaru.Dostawca), kontrahent);
            if(towar.Dostawcy[rc].GetFirst() is DostawcaTowaru dostawca)
            {
                return dostawca.GetPurchasePriceNetto();
            }
            return Currency.Empty;
        }

        public Currency GetPurchasePriceBrutto(Towar towar, Kontrahent kontrahent)
        {
            RowCondition rc = new FieldCondition.Equal(nameof(DostawcaTowaru.Dostawca), kontrahent);
            if (towar.Dostawcy[rc].GetFirst() is DostawcaTowaru dostawca)
            {
                return dostawca.GetPurchasePriceBrutto();
            }
            return Currency.Empty;
        }

		public Kontrahent GetDefaultDostawcaKontrahent(Towar towar)
        {
            RowCondition rc = new FieldCondition.Equal(nameof(DostawcaTowaru.Towar), towar);
            rc &= new FieldCondition.Equal(nameof(DostawcaTowaru.Domyslny), true);

            if (_session.GetTowary().DostawcyTowaru.PrimaryKey[rc].GetFirst() is DostawcaTowaru dostawcaTowaru)
            {
                return dostawcaTowaru.Dostawca;
            }
            return null;
        }

        public bool IsKontrahentDostawcaTowaru(Kontrahent kontrahent, Towar towar)
        {
			RowCondition rc = new FieldCondition.Equal(nameof(DostawcaTowaru.Towar), towar);
			rc &= new FieldCondition.Equal(nameof(DostawcaTowaru.Dostawca), kontrahent);
            return _session.GetTowary().DostawcyTowaru.PrimaryKey[rc].Any;
		}
    }
}
