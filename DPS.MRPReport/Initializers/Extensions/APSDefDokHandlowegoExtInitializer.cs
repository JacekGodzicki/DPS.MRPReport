using DPS.MRPReport.Initializers.Extensions;
using DPS.MRPReport.Extensions;
using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.Handel;
using DPS.MRPReport.Rows.Extensions;

[assembly: ProgramInitializer(typeof(APSDefDokHandlowegoExtInitializer))]

namespace DPS.MRPReport.Initializers.Extensions
{
	internal class APSDefDokHandlowegoExtInitializer : IProgramInitializer
    {
        public void Initialize()
        {
            HandelModule.DefDokHandlowegoSchema.AddOnAdded(OnAddedHandle);
			HandelModule.DefDokHandlowegoSchema.AddOnCalcObjectRight(OnCalcObjectRightHandle);
		}

		private void OnAddedHandle(HandelModule.DefDokHandlowegoRow row)
        {
            DefDokHandlowego defDokHandlowego = (DefDokHandlowego)row;
            new RowExtensionUtil<DefDokHandlowego, APSDefDokHandlowegoExt>().CreateExtension(defDokHandlowego);
        }

		private void OnCalcObjectRightHandle(HandelModule.DefDokHandlowegoRow row, ref AccessRights accessRights)
		{
			DefDokHandlowego defDokHandlowego = (DefDokHandlowego)row;
			APSDefDokHandlowegoExt defDokHandlowegoExt = defDokHandlowego.GetAPSExt();
			if(accessRights == AccessRights.Denied)
			{
				if(defDokHandlowegoExt == null)
				{
					accessRights = AccessRights.ReadOnly;
				}
				else if(defDokHandlowegoExt.State == RowState.Added)
				{
					accessRights = AccessRights.Granted;
				}
			}
		}
	}
}
