using DPS.MRPReport.Configurations;
using DPS.MRPReport.Enums;
using DPS.MRPReport.Rows.Extensions;
using Soneta.Magazyny;
using Soneta.Towary;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.ReportMRPGenerator.Models
{
	public class APSReportMRPPositionModel
	{
		public APSReportMRPPositionModel Parent { get; set; }
		public APSReportMRPPositionModel RelatedSuggestion { get; set; }
		public APSTowarExt TowarExt { get; set; }
		public Date StartDate { get; set; }
		public Date AvailabilityDate { get; set; }
		public Quantity BalanceQuantity { get; set; }
		public Quantity BalanceQuantityCalculations { get; set; }
		public string DefinitionCode { get; set; }
		public KierunekPartii Direction { get; set; }
		public string DocumentNumber { get; set; }
		public ObtainingMethodEnum ObtainingMethod { get; set; }
		public int ObtainingPeriod { get; set; }
		public Quantity Quantity { get; set; }

		public bool IsSuggestion()
		{
			return DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.SP
				|| DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.SZ;
		}

		public bool IsProductionSuggestion()
		{
			return DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.SP;
		}
	}
}