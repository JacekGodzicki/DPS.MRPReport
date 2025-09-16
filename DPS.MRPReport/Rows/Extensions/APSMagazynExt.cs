using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.Magazyny;

namespace DPS.MRPReport.Rows.Extensions
{
	public class APSMagazynExt : DPSMRPReportModule.APSMagazynExtRow
	{
		public APSMagazynExt(RowCreator creator) : base(creator)
		{
		}

		public APSMagazynExt([Required] Magazyn magazyn) : base(magazyn)
		{
		}

		protected override void OnDeleting()
		{
			new RowExtensionUtil<Magazyn, APSMagazynExt>().DeletingWithoutExtendedObjectNotAllowed(Magazyn, this);
			base.OnDeleting();
		}

		public override bool IsReadOnly()
			=> Magazyn.IsReadOnly();

		public override string ToString()
		{
			return Magazyn.ToString();
		}
	}
}