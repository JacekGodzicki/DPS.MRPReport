using DPS.MRPReport.Rows.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Tables.Extensions
{
	public class APSPozDHExt : DPSMRPReportModule.APSPozycjaDokHandlowegoExtTable
	{
		protected override string[] GetDefaultLocatorFields()
		{
			string locator = $"{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego)}.{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego.NazwaIndywidualna)}";
			List<string> locators = base.GetDefaultLocatorFields().ToList();
			if(!locators.Contains(locator))
			{
				locators.Add(locator);
			}
			return locators.ToArray();
		}
	}
}
