using DPS.MRPReport.Workers.Config.Abstractions;
using Soneta.Business;

namespace DPS.MRPReport.Workers.Config
{
    public class ConfigReportMRPMagazynyWorker : ConfigurationNodesManager
    {
        public ConfigReportMRPMagazynyWorker(Session session)
        {
            Session = session;
        }

        public ViewInfo APSMagExtViewInfo
        {
            get
            {
                ViewInfo viewInfo = new ViewInfo();
                viewInfo.CreateView += APSMagExtViewInfoCreateView;
                viewInfo.AllowNewInPlace = false;
                return viewInfo;
            }
        }

        private void APSMagExtViewInfoCreateView(object sender, CreateViewEventArgs args)
        {
            args.View = args.Session.GetDPSMRPReport().APSMagExt.CreateView();
            args.View.AllowEdit = true;
            args.View.AllowUpdate = true;
            args.View.AllowRemove = false;
            args.View.AllowNew = false;
        }
    }
}
