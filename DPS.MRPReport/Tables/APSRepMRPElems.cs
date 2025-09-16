using DPS.MRPReport.Rows;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Tables
{
	public class APSRepMRPElems : DPSMRPReportModule.APSReportMRPElementTable
	{
		protected override string[] GetDefaultLocatorFields()
		{
			string locator = $"{nameof(APSReportMRPElement.Towar)}.{nameof(APSReportMRPElement.Towar.Kod)}";
			List<string> locators = base.GetDefaultLocatorFields().ToList();
			if(!locators.Contains(locator))
			{
				locators.Add(locator);
			}
			return locators.ToArray();
		}
	}
}
