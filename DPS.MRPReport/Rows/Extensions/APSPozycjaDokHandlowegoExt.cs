using DPS.MRPReport.Extensions;
using DPS.MRPReport.Utils;
using Soneta.Business;
using Soneta.Handel;
using Soneta.Towary;
using Soneta.Types;
using System.Linq;

namespace DPS.MRPReport.Rows.Extensions
{
	public class APSPozycjaDokHandlowegoExt : DPSMRPReportModule.APSPozycjaDokHandlowegoExtRow
	{
		public APSPozycjaDokHandlowegoExt(RowCreator creator) : base(creator)
		{
		}

		public APSPozycjaDokHandlowegoExt([Required] PozycjaDokHandlowego pozycjadokhandlowego) : base(pozycjadokhandlowego)
		{
		}

		[AttributeInheritance]
		public override Date DeliveryDate
		{
			get => base.DeliveryDate;
			set
			{
				base.DeliveryDate = value;
				SetDataDostawy();
			}
		}

		[Caption("Minimum logistyczne")]
		public Quantity LogisticalMinimum
		{
			get
			{
				if(GetDostawcaTowaru() is DostawcaTowaru dostawca)
				{
					return dostawca.GetAPSExt().LogisticalMinimum;
				}
				return Quantity.Empty;
			}
		}

		private DostawcaTowaru GetDostawcaTowaru()
		{
			

			if(base.PozycjaDokHandlowego.Dokument.Kontrahent is null
				|| base.PozycjaDokHandlowego.Towar is null)
			{
				return null;
			}

			RowCondition rc = new FieldCondition.Equal(nameof(DostawcaTowaru.Towar), base.PozycjaDokHandlowego.Towar);
			rc &= new FieldCondition.Equal(nameof(DostawcaTowaru.Dostawca), base.PozycjaDokHandlowego.Dokument.Kontrahent);
			return base.PozycjaDokHandlowego.Towar.Dostawcy[rc].GetFirst();
		}

		private void SetDataDostawy()
		{
			Date terminDostawy = PozycjaDokHandlowego.Dokument
				.Pozycje
				.Select(x => x.GetAPSExt().DeliveryDate)
				.Where(x => x != Date.Empty)
				.Order()
				.FirstOrDefault();

			if(terminDostawy == Date.Empty)
			{
				return;
			}

			PozycjaDokHandlowego.Dokument.Dostawa.Termin = terminDostawy;
		}

		protected override void OnDeleting()
		{
			new RowExtensionUtil<PozycjaDokHandlowego, APSPozycjaDokHandlowegoExt>().DeletingWithoutExtendedObjectNotAllowed(PozycjaDokHandlowego, this);
			base.OnDeleting();
		}

		public override bool IsReadOnly()
			=> PozycjaDokHandlowego.IsReadOnly();

		public override string ToString()
		{
			return PozycjaDokHandlowego.ToString();
		}
	}
}