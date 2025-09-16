using DPS.MRPReport.UI.Extenders.Config;
using DPS.MRPReport.UI.Extenders.Config.Abstractions;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;

[assembly: Worker(typeof(ConfigReporMRPMainExtender))]

namespace DPS.MRPReport.UI.Extenders.Config
{
	public class ConfigReporMRPMainExtender : ConfigExtenderBase<ConfigReportMRPMainWorker>
    {
        public ConfigReporMRPMainExtender(Session session) : base(session)
        {
        }
    }
}