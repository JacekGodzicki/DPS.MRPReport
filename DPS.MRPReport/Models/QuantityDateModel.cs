using DPS.MRPReport.Rows;
using Soneta.Towary;
using Soneta.Types;

namespace DPS.MRPReport.Models
{
	public class QuantityDateModel
	{
		public Date StartDate { get; set; }
		public Date AvailabilityDate { get; set; }
		public Quantity Quantity { get; set; }
		public APSReportMRPPosition[] SuggestionPositions { get; set; } = [];
	}
}
