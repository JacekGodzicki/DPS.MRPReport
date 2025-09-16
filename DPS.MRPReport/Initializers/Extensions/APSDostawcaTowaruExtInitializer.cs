using DPS.MRPReport.Initializers.Extensions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.Towary;

[assembly: ProgramInitializer(typeof(APSDostawcaTowaruExtInitializer))]

namespace DPS.MRPReport.Initializers.Extensions
{
	internal class APSDostawcaTowaruExtInitializer : IProgramInitializer
    {
        public void Initialize()
        {
            TowaryModule.DostawcaTowaruSchema.AddOnAdded(CreateExtension);
        }

        private void CreateExtension(TowaryModule.DostawcaTowaruRow row)
        {
            DostawcaTowaru dostawcaTowaru = (DostawcaTowaru)row;
            new RowExtensionUtil<DostawcaTowaru, APSDostawcaTowaruExt>().CreateExtension(dostawcaTowaru);
        }
    }
}