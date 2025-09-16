using DPS.MRPReport.UI.Extenders.Config;
using DPS.MRPReport.UI.Extenders.Config.Abstractions;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;

[assembly: Worker(typeof(ConfigReportMRPMagazynyExtender))]

namespace DPS.MRPReport.UI.Extenders.Config
{
    public class ConfigReportMRPMagazynyExtender : ConfigExtenderBase<ConfigReportMRPMagazynyWorker>
    {
        public ConfigReportMRPMagazynyExtender(Session session) : base(session)
        {
        }
    }
}
