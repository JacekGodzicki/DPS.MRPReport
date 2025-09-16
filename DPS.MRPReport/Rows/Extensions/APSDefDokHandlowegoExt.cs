using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.Handel;

namespace DPS.MRPReport.Rows.Extensions
{
	public class APSDefDokHandlowegoExt : DPSMRPReportModule.APSDefDokHandlowegoExtRow
	{
		public APSDefDokHandlowegoExt(RowCreator creator) : base(creator)
		{
		}

		public APSDefDokHandlowegoExt([Required] DefDokHandlowego defdokhandlowego) : base(defdokhandlowego)
		{
		}

		protected override void OnDeleting()
		{
			new RowExtensionUtil<DefDokHandlowego, APSDefDokHandlowegoExt>().DeletingWithoutExtendedObjectNotAllowed(DefDokHandlowego, this);
			base.OnDeleting();
		}

		public override bool IsReadOnly()
			=> DefDokHandlowego.IsReadOnly();

		public override string ToString()
		{
			return DefDokHandlowego.ToString();
		}
	}
}