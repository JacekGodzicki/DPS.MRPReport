using DPS.MRPReport.Initializers.Extensions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.Handel;

[assembly: ProgramInitializer(typeof(APSPozycjaDokHandlowegoInitializer))]

namespace DPS.MRPReport.Initializers.Extensions
{
	internal class APSPozycjaDokHandlowegoInitializer : IProgramInitializer
    {
        public void Initialize()
        {
            HandelModule.PozycjaDokHandlowegoSchema.AddOnAdded(OnAddedHandle);
        }

        private void OnAddedHandle(HandelModule.PozycjaDokHandlowegoRow row)
        {
            PozycjaDokHandlowego pozycjaDokHandlowego = (PozycjaDokHandlowego)row;
            new RowExtensionUtil<PozycjaDokHandlowego, APSPozycjaDokHandlowegoExt>().CreateExtension(pozycjaDokHandlowego);
        }
    }
}
