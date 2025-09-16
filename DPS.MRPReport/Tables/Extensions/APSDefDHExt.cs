using DPS.MRPReport.Rows.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Tables.Extensions
{
	public class APSDefDHExt : DPSMRPReportModule.APSDefDokHandlowegoExtTable
	{
		protected override string[] GetDefaultLocatorFields()
		{
			string locator = $"{nameof(APSDefDokHandlowegoExt.DefDokHandlowego)}.{nameof(APSDefDokHandlowegoExt.DefDokHandlowego.Symbol)}";
			List<string> locators = base.GetDefaultLocatorFields().ToList();
			if(!locators.Contains(locator))
			{
				locators.Add(locator);
			}
			return locators.ToArray();
		}
	}
}
