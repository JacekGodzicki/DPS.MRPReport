using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Workers.Config.Abstractions;
using Soneta.Business;
using Soneta.Towary;

namespace DPS.MRPReport.Workers.Config
{
	public class ConfigReportMRPTowaryWorker : ConfigurationNodesManager
    {
        public ConfigReportMRPTowaryWorker(Session session)
        {
            Session = session;
        }

        public ViewInfo APSTowaryExtViewInfo
        {
            get
            {
                ViewInfo viewInfo = new ViewInfo();
                viewInfo.CreateView += APSTowaryExtViewInfoCreateView;
				viewInfo.Action += APSTowaryExtViewInfoAction;
				viewInfo.AllowNewInPlace = false;
                return viewInfo;
            }
        }

		private void APSTowaryExtViewInfoCreateView(object sender, CreateViewEventArgs args)
        {
            args.View = GetViewAPSTowaryExt(args.Session);
			args.View.AllowEdit = true;
            args.View.AllowUpdate = true;
            args.View.AllowRemove = false;
            args.View.AllowNew = false;
        }

		private void APSTowaryExtViewInfoAction(object sender, ActionEventArgs e)
		{
			if(e.FocusedData is APSTowarExt towarExt)
			{
				e.FocusedData = towarExt.Towar;
			}
		}

		private View GetViewAPSTowaryExt(Session session)
        {
            RowCondition rc = new FieldCondition.In($"{nameof(APSTowarExt.Towar)}.{nameof(APSTowarExt.Towar.Typ)}", [TypTowaru.Towar, TypTowaru.Produkt]);
            return session.GetDPSMRPReport().APSTowaryExt.PrimaryKey[rc].CreateView();

		}
	}
}
