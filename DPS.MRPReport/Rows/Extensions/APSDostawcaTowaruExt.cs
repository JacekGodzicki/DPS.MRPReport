using DPS.MRPReport.Enums;
using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.Towary;

namespace DPS.MRPReport.Rows.Extensions
{
	public class APSDostawcaTowaruExt : DPSMRPReportModule.APSDostawcaTowaruExtRow
	{
		public APSDostawcaTowaruExt(RowCreator creator) : base(creator)
		{
		}

		public APSDostawcaTowaruExt([Required] DostawcaTowaru dostawcatowaru) : base(dostawcatowaru)
		{
		}

		protected override void OnAdded()
		{
			if(DostawcaTowaru?.Towar?.Jednostka is Jednostka jednostka)
			{
				base.LogisticalMinimum = new Quantity(0, jednostka.Kod);
			}
			base.QualityAssessment = QualityAssessmentEnum.Assessment1;
			base.OnAdded();
		}

		protected override void OnDeleting()
		{
			new RowExtensionUtil<DostawcaTowaru, APSDostawcaTowaruExt>().DeletingWithoutExtendedObjectNotAllowed(DostawcaTowaru, this);
			base.OnDeleting();
		}

		[AttributeInheritance]
		public override Quantity LogisticalMinimum
		{
			get => base.LogisticalMinimum;
			set
			{
				if(value == Quantity.Empty)
				{
					base.LogisticalMinimum = new Quantity(0, DostawcaTowaru.Towar.Jednostka.Kod);
					return;
				}

				if(string.IsNullOrWhiteSpace(value.Symbol))
				{
					base.LogisticalMinimum = new Quantity(value.Value, DostawcaTowaru.Towar.Jednostka.Kod);
					return;
				}
				base.LogisticalMinimum = value;
			}
		}

		public override bool IsReadOnly()
			=> DostawcaTowaru.IsReadOnly();

		public override string ToString()
		{
			return DostawcaTowaru.ToString();
		}
	}
}