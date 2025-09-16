using DPS.MRPReport.Rows.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Tables.Extensions
{
	public class APSTowaryExt : DPSMRPReportModule.APSTowarExtTable
	{
		protected override string[] GetDefaultLocatorFields()
		{
			string locator = $"{nameof(APSTowarExt.Towar)}.{nameof(APSTowarExt.Towar.Kod)}";
			List<string> locators = base.GetDefaultLocatorFields().ToList();
			if(!locators.Contains(locator))
			{
				locators.Add(locator);
			}
			return locators.ToArray();
		}
	}
}