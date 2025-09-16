using DPS.MRPReport.Rows.Extensions;
using Soneta.Business;
using Soneta.Handel.workers;
using Soneta.Towary;
using Soneta.Types;

namespace DPS.MRPReport.Extensions
{
	public static class DostawcaTowaruExtension
    {
        public static APSDostawcaTowaruExt GetAPSExt(this DostawcaTowaru row) => row?.Session?.GetDPSMRPReport().APSDostTowExt.WgDostawcaTowaru[row];
        
        public static Currency GetPurchasePriceNetto(this DostawcaTowaru row)
        {
            Context context = Context.Empty.Clone(row.Session);
            context.Set(row.Towar);

            CennikDostawcyWorker cennikDostawcyWorker = new CennikDostawcyWorker();
            cennikDostawcyWorker.DostawcaTowaru = row;
            cennikDostawcyWorker.Context = context;
            return cennikDostawcyWorker.Netto;
        }

        public static Currency GetPurchasePriceBrutto(this DostawcaTowaru row)
        {
            Context context = Context.Empty.Clone(row.Session);
            context.Set(row.Towar);

            CennikDostawcyWorker cennikDostawcyWorker = new CennikDostawcyWorker();
            cennikDostawcyWorker.DostawcaTowaru = row;
            cennikDostawcyWorker.Context = context;
            return cennikDostawcyWorker.Brutto;
        }

    }
}
