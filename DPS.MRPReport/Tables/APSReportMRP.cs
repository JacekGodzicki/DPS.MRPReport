using DPS.MRPReport.Rows;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Tables
{
	public class APSReportMRP : DPSMRPReportModule.APSReportMRPPositionTable
	{
		protected override string[] GetDefaultLocatorFields()
		{
			string locator = $"{nameof(APSReportMRPPosition.ReportMRPElement)}.{nameof(APSReportMRPPosition.ReportMRPElement.Towar)}.{nameof(APSReportMRPPosition.ReportMRPElement.Towar.Kod)}";
			List<string> locators = base.GetDefaultLocatorFields().ToList();
			if(!locators.Contains(locator))
			{
				locators.Add(locator);
			}
			return locators.ToArray();
		}
	}
}