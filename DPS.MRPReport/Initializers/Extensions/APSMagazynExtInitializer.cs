using DPS.MRPReport.Extensions;
using DPS.MRPReport.Initializers.Extensions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.Magazyny;

[assembly: ProgramInitializer(typeof(APSMagazynExtInitializer))]

namespace DPS.MRPReport.Initializers.Extensions
{
	internal class APSMagazynExtInitializer : IProgramInitializer
	{
		public void Initialize()
		{
			MagazynyModule.MagazynSchema.AddOnAdded(OnAddedHandle);
			MagazynyModule.MagazynSchema.AddOnCalcObjectRight(OnCalcObjectRightHandle);

		}

		private void OnAddedHandle(MagazynyModule.MagazynRow row)
		{
			Magazyn magazyn = (Magazyn)row;
			new RowExtensionUtil<Magazyn, APSMagazynExt>().CreateExtension(magazyn);
		}

		private void OnCalcObjectRightHandle(MagazynyModule.MagazynRow row, ref AccessRights accessRights)
		{
			Magazyn magazyn = (Magazyn)row;
			APSMagazynExt apsMagazynExt = magazyn.GetAPSExt();
			if(accessRights == AccessRights.Denied)
			{
				if(apsMagazynExt == null)
				{
					accessRights = AccessRights.ReadOnly;
				}
				else if(apsMagazynExt.State == RowState.Added)
				{
					accessRights = AccessRights.Granted;
				}
			}
		}
	}
}
