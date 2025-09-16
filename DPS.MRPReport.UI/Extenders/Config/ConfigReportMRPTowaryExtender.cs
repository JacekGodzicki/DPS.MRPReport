using Soneta.Business;
using DPS.MRPReport.UI.Extenders.Config;
using DPS.MRPReport.UI.Workers.SelectionProductGroup;
using DPS.MRPReport.Workers.Config;
using DPS.MRPReport.UI.Extenders.Config.Abstractions;

[assembly: Worker(typeof(ConfigReportMRPTowaryExtender))]

namespace DPS.MRPReport.UI.Extenders.Config
{
    public class ConfigReportMRPTowaryExtender : ConfigExtenderBase<ConfigReportMRPTowaryWorker>
    {
		[Context]
		public Context Context { get; set; }

        public ConfigReportMRPTowaryExtender(Session session) : base(session)
        {
        }

		public object SelectProductGroups()
		{
			SelectionProductGroupLogic logic = new SelectionProductGroupLogic(Context);
			return logic.GetUI();
		}

		public object UnselectProductGroups()
		{
			UnselectionProductGroupLogic logic = new UnselectionProductGroupLogic(Context);
			return logic.GetUI();
		}
	}
}
