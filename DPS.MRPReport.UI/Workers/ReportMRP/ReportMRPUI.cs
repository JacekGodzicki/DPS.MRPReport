using DPS.MRPReport.Rows;
using DPS.MRPReport.UI.Workers.ReportMRP;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Types;
using System;

[assembly: Worker(typeof(ReportMRPUI))]
[assembly: FolderView("Smart APS/Raport MRP",
	Priority = 10,
	ForeColor = 2377368,
	Description = "Raport MRP",
	ReadOnlySession = true,
	ConfigSession = false,
	ObjectType = typeof(ReportMRPUI))]

namespace DPS.MRPReport.UI.Workers.ReportMRP
{
	public class ReportMRPUI : ContextBase
    {
        private static APSReportMRPElement[] _reportElements = [];
        private static APSReportMRPElement[] _selectedReportElements = [];

        private ViewInfo _reportMRPGridViewInfo;
        private UIElement _reportMRPGridUIElement;

        public ReportMRPUI(Context context) : base(context)
        {
            InitializeParams(context);
        }

        public APSReportMRPElement[] ReportElements => _reportElements;

        public APSReportMRPElement[] SelectedReportElements
        {
            get => _selectedReportElements;
            set => _selectedReportElements = value;
        }

        public ViewInfo ReportMRPGridViewInfo
        {
            get
            {
                if (_reportMRPGridViewInfo == null)
                {
                    _reportMRPGridViewInfo = new ViewInfo();
                    _reportMRPGridViewInfo.AllowNewInPlace = false;
                    _reportMRPGridViewInfo.ReadOnly = true;
                    _reportMRPGridViewInfo.CreateView += ReportMRPGridViewInfoCreateView;
                }
                return _reportMRPGridViewInfo;
            }
        }

        public UIElement ReportMRPGridUIElement
        {
            get
            {
                if (_reportMRPGridUIElement is null)
                {
                    _reportMRPGridUIElement = CreateReportMRPGridUIElement();
                }
                return _reportMRPGridUIElement;
            }
        }

        private void InitializeParams(Context context)
        {
            if (context.Contains(typeof(ReportMRPUIParams)))
            {
                context.Remove(typeof(ReportMRPUIParams));
            }

            ReportMRPUIParams pars = new ReportMRPUIParams(context);
            context.Set(pars);
            pars.Changed -= ParametryChanged;
            pars.Changed += ParametryChanged;
        }

        private void ReportMRPGridViewInfoCreateView(object sender, CreateViewEventArgs args)
        {
            args.View = new View(args.Session.GetDPSMRPReport().APSRepMRPElems.PrimaryKey, _reportElements);
			args.View.AllowEdit = true;
            args.View.AllowUpdate = false;
            args.View.AllowNew = false;
            args.View.AllowRemove = false;
        }

        private void ParametryChanged(object sender, EventArgs e)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            _reportMRPGridUIElement = CreateReportMRPGridUIElement();
            _reportElements = GetReportMRPElements();
        }

        private UIElement CreateReportMRPGridUIElement()
        {
            ReportMRPUIParams reportMRPUIParams = Context[typeof(ReportMRPUIParams), false] as ReportMRPUIParams;
            ReportMRPGridGenerator.Params pars = new ReportMRPGridGenerator.Params
            {
                Dates = reportMRPUIParams?.GetGroupDates() ?? [],
                SelectedDays = reportMRPUIParams?.SelectedDates ?? [],
                EditValuePropertyName = nameof(ReportMRPGridViewInfo),
                SelectedValuePropertyName = nameof(SelectedReportElements),
            };
            return new ReportMRPGridGenerator(pars).Generate();
        }

        private APSReportMRPElement[] GetReportMRPElements()
        {
            ReportMRPUIParams parametry = Context[typeof(ReportMRPUIParams), false] as ReportMRPUIParams;
            Date[] dates = parametry?.GetGroupDates() ?? [];
            ReportMRPLogic logic = new ReportMRPLogic(Session, dates, parametry);
            return logic.GetCalculatedReportMRPElements();
        }
    }
}