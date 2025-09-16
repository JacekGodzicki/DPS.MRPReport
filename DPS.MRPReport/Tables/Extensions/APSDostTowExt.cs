using DPS.MRPReport.Rows.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Tables.Extensions
{
	public class APSDostTowExt : DPSMRPReportModule.APSDostawcaTowaruExtTable
	{
		protected override string[] GetDefaultLocatorFields()
		{
			string locator = $"{nameof(APSDostawcaTowaruExt.DostawcaTowaru)}.{nameof(APSDostawcaTowaruExt.DostawcaTowaru.Dostawca)}.{nameof(APSDostawcaTowaruExt.DostawcaTowaru.Dostawca.Kod)}";
			List<string> locators = base.GetDefaultLocatorFields().ToList();
			if(!locators.Contains(locator))
			{
				locators.Add(locator);
			}
			return locators.ToArray();
		}
	}
}
