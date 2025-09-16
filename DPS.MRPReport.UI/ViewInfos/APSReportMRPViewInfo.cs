using Soneta.Business;
using Soneta.Business.UI;
using System.ComponentModel;

[assembly: FolderView("Smart APS",
	Priority = 1,
	ForeColor = 2377368,
	Description = "Smart APS",
	ReadOnlySession = false,
	ConfigSession = false)]

namespace DPS.MRPReport.UI.ViewInfos
{
	public class APSReportMRPViewInfo : ViewInfo
    {
        public APSReportMRPViewInfo()
        {
            InitializeComponent();
        }

        public APSReportMRPViewInfo(IContainer container) : base(container)
        {
            container.Add(this);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
			ResourceName = nameof(APSReportMRPViewInfo);
			AllowNewInPlace = false;
			CreateView += RapMRPViewInfoCreateView;
        }

		private void RapMRPViewInfoCreateView(object sender, CreateViewEventArgs args)
        {
            args.View = args.Session.GetDPSMRPReport().APSReportMRP.CreateView();
            args.View.AllowNew = false;
            args.View.AllowRemove = true;
            args.View.AllowUpdate = false;
            args.View.AllowEdit = true;
        }
    }
}