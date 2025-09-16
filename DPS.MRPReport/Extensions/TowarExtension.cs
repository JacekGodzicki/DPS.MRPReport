using DPS.MRPReport.Rows.Extensions;
using Soneta.Business;
using Soneta.ProdukcjaPro;
using Soneta.Towary;
using Soneta.Types;

namespace DPS.MRPReport.Extensions
{
	public static class TowarExtension
    {
        public static APSTowarExt GetAPSExt(this Towar row) => row?.Session?.GetDPSMRPReport().APSTowaryExt.WgTowar[row];

		public static ProTechnologia GetDefaultTechnology(this Towar towar)
		{
			RowCondition rc = new FieldCondition.Equal(nameof(ProTechnologia.Domyslna), true);
			rc &= new FieldCondition.Equal(nameof(ProTechnologia.Stan), ProStanTechnologii.Zatwierdzona);
			rc &= new FieldCondition.Equal(nameof(ProTechnologia.Towar), towar);
			return towar.Session.GetProdukcjaPro().ProTechnologie.PrimaryKey[rc].GetFirst() as ProTechnologia;
		}
	}
}