using Soneta.Business;
using Soneta.ProdukcjaPro;
using Soneta.Towary;
using System.Linq;

namespace DPS.MRPReport.Utils
{
    public class ProTechnologiaUtil
    {
        private Session _session;

        public ProTechnologiaUtil(Session session)
        {
            _session = session;
        }

        public ProTechnologia GetDefaultTechnologia(Towar towar)
        {
            RowCondition rc = GetDefaultTechnologiaRowCondition(towar);
            return _session.GetProdukcjaPro().ProTechnologie.PrimaryKey[rc].GetFirst() as ProTechnologia;
        }

        public ProTechnologia[] GetTechnologie(Towar towar)
        {
            RowCondition rc = GetDefaultTechnologiaRowCondition(towar);
            return _session.GetProdukcjaPro().ProTechnologie.PrimaryKey[rc]
                .Cast<ProTechnologia>()
                .ToArray();
        }

        public bool HasDefaultTechnologia(Towar towar)
        {
            RowCondition rc = GetDefaultTechnologiaRowCondition(towar);
            return _session.GetProdukcjaPro().ProTechnologie.PrimaryKey[rc].Any;
        }

        private RowCondition GetTechnologieRowCondition(Towar towar)
        {
            RowCondition rc = new FieldCondition.Equal(nameof(ProTechnologia.Towar), towar);
            rc &= new FieldCondition.Equal(nameof(ProTechnologia.Stan), ProStanTechnologii.Zatwierdzona);
            return rc;
        }

        private RowCondition GetDefaultTechnologiaRowCondition(Towar towar)
        {
            RowCondition rc = new FieldCondition.Equal(nameof(ProTechnologia.Towar), towar);
            rc &= new FieldCondition.Equal(nameof(ProTechnologia.Stan), ProStanTechnologii.Zatwierdzona);
            rc &= new FieldCondition.Equal(nameof(ProTechnologia.Domyslna), true);
            return rc;
        }
    }
}
