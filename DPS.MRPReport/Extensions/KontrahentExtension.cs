using Soneta.Business;
using Soneta.CRM;
using Soneta.Towary;
using System.Linq;

namespace DPS.MRPReport.Extensions
{
    public static class KontrahentExtension
    {
        public static bool IsDostawcaTowaru(this Kontrahent kontrahent, Towar towar)
        {
            DostawcaTowaru dostawcaTowaru = kontrahent.Session.GetTowary().DostawcyTowaru.Rows.Changed
                .OfType<DostawcaTowaru>()
                .LastOrDefault();

            if (dostawcaTowaru is not null)
            {
                return !dostawcaTowaru.IsDeletedOrDetached();
            }

            RowCondition rc = new FieldCondition.Equal(nameof(DostawcaTowaru.Dostawca), kontrahent);
            rc &= new FieldCondition.Equal(nameof(DostawcaTowaru.Towar), towar);
            return kontrahent.Session.GetTowary().DostawcyTowaru.PrimaryKey[rc].Any;
        }
    }
}