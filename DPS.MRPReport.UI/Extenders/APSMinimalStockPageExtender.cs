using DPS.MRPReport.Rows;
using DPS.MRPReport.UI.Extenders;
using Soneta.Business;
using Soneta.Towary;

[assembly: Worker(typeof(APSMinimalStockPageExtender))]

namespace DPS.MRPReport.UI.Extenders
{
    internal class APSMinimalStockPageExtender
    {
        [Context]
        public Towar Towar { get; set; }

        public ViewInfo PositionsViewInfo
        {
            get
            {
                ViewInfo viewInfo = new ViewInfo();
                viewInfo.CreateView += APSMinimalStockPageViewInfoCreateView;
                return viewInfo;
            }
        }

        private void APSMinimalStockPageViewInfoCreateView(object sender, CreateViewEventArgs args)
        {
            RowCondition rc = new FieldCondition.Equal($"{nameof(APSMinimalStock.Towar)}", Towar);
            args.View = args.Session.GetDPSMRPReport().APSMinStocks.PrimaryKey[rc].CreateView();
        }
    }
}