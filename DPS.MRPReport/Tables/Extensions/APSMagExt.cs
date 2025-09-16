using DPS.MRPReport.Rows.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Tables.Extensions
{
	public class APSMagExt : DPSMRPReportModule.APSMagazynExtTable
	{
		protected override string[] GetDefaultLocatorFields()
		{
			string locator = $"{nameof(APSMagazynExt.Magazyn)}.{nameof(APSMagazynExt.Magazyn.Symbol)}";
			List<string> locators = base.GetDefaultLocatorFields().ToList();
			if(!locators.Contains(locator))
			{
				locators.Add(locator);
			}
			return locators.ToArray();
		}
	}
}
