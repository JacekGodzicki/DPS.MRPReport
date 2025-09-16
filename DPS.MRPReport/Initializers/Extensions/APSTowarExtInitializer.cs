using DPS.MRPReport.Initializers.Extensions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.Towary;

[assembly: ProgramInitializer(typeof(APSTowarExtInitializer))]

namespace DPS.MRPReport.Initializers.Extensions
{
	internal class APSTowarExtInitializer : IProgramInitializer
    {
        public void Initialize()
        {
            TowaryModule.TowarSchema.AddOnAdded(OnAddedHandle);
        }

        private void OnAddedHandle(TowaryModule.TowarRow row)
        {
            Towar towar = (Towar)row;
            new RowExtensionUtil<Towar, APSTowarExt>().CreateExtension(towar);
        }
    }
}