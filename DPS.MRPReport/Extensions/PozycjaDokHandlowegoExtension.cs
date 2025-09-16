using DPS.MRPReport.Rows.Extensions;
using Soneta.Handel;

namespace DPS.MRPReport.Extensions
{
	public static class PozycjaDokHandlowegoExtension
    {
        public static APSPozycjaDokHandlowegoExt GetAPSExt(this PozycjaDokHandlowego row) => row?.Session?.GetDPSMRPReport().APSPozDHExt.WgPozycjaDokHandlowego[row];
    }
}