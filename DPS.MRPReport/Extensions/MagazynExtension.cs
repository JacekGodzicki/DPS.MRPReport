using DPS.MRPReport.Rows.Extensions;
using Soneta.Magazyny;

namespace DPS.MRPReport.Extensions
{
	public static class MagazynExtension
    {
        public static APSMagazynExt GetAPSExt(this Magazyn row) => row?.Session?.GetDPSMRPReport().APSMagExt.WgMagazyn[row];
    }
}