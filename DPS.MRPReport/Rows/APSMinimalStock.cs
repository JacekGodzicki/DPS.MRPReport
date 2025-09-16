using DPS.MRPReport.Rows;
using Soneta.Business;
using Soneta.Towary;
using Soneta.Types;
using System;
using System.Linq;

[assembly: NewRow("Okresowy stan minimalny", typeof(APSMinimalStock))]

namespace DPS.MRPReport.Rows
{
	[Caption("Okresowy stan minimalny")]
	public class APSMinimalStock : DPSMRPReportModule.APSMinimalStockRow
	{
		public APSMinimalStock(RowCreator creator) : base(creator)
		{
		}

		public APSMinimalStock([Required] Towar towar) : base(towar)
		{
		}

		[AttributeInheritance]
		public override FromTo Period
		{
			get => base.Period;
			set
			{
				if(CheckPeriodValidity(value))
				{
					throw new Exception("Okresy nie mogą się przecinać");
				}
				base.Period = value;
			}
		}

		[AttributeInheritance]
		public override Quantity Quantity
		{
			get => base.Quantity;
			set
			{
				if(value.Value < 0)
				{
					base.Quantity = new Quantity(0, base.Towar.Jednostka.Kod);
				}
				else
				{
					base.Quantity = value;
				}
			}
		}

		private bool CheckPeriodValidity(FromTo fromTo)
		{
			RowCondition rc = new FieldCondition.Equal(nameof(APSMinimalStock.Towar), Towar);
			rc &= RowCondition.IsIntersected(nameof(Period), fromTo);
			return Session.GetDPSMRPReport().APSMinStocks.PrimaryKey[rc].CreateView().Any();
		}

		protected override void OnAdded()
		{
			base.Period = FromTo.Month(Date.Today);
			base.Quantity = new(0, base.Towar.Jednostka.Kod);
		}

		public override string ToString()
		{
			return $"{base.Towar}: ({base.Period.From} - {base.Period.To}) - {base.Quantity} ";
		}
	}
}