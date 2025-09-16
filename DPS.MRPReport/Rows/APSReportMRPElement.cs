using DPS.MRPReport.Extensions;
using DPS.MRPReport.Models;
using Soneta.Business;
using Soneta.Towary;
using Soneta.Types;

namespace DPS.MRPReport.Rows
{
	[Caption("Element raportu MRP")]
	public class APSReportMRPElement : DPSMRPReportModule.APSReportMRPElementRow
	{
		public APSReportMRPElement(RowCreator creator) : base(creator)
		{
		}

		public APSReportMRPElement([Required] Towar towar) : base(towar)
		{
			base.baseObtainingMethod = towar.GetAPSExt().MRPObtainingMethod;
		}

		[Caption("Saldo końcowe")]
		public Quantity FinalBalance { get; set; }

		[Caption("Nadwyżka")]
		public Quantity Overcapacity { get; set; }

		public QuantityDateModel[] QuantityDates { get; set; }
		
		public override string ToString()
		{
			return Towar?.ToString() ?? string.Empty;
		}
	}
}