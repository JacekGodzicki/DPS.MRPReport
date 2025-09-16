using DPS.MRPReport.Rows.Extensions;
using Soneta.Handel;

namespace DPS.MRPReport.Extensions
{
	public static class DefDokHandlowegoExtension
    {
        public static APSDefDokHandlowegoExt GetAPSExt(this DefDokHandlowego row) => row?.Session?.GetDPSMRPReport().APSDefDHExt.WgDefDokHandlowego[row];
    }
}
