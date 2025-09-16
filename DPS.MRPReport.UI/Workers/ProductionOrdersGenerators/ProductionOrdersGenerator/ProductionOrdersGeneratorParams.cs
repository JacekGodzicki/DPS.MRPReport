using DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersCreator.Models;
using Soneta.Business;
using Soneta.ProdukcjaPro;
using Soneta.Tools;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersGenerator
{
	public class ProductionOrdersGeneratorParams : ContextBase
	{
		private TowarModelContextBase _towarModelContextBase;
		private ProWydzial _wydzial;

		public ProductionOrdersGeneratorParams(Context context) : base(context)
		{
			_towarModelContextBase = context[typeof(TowarModelContextBase), false] as TowarModelContextBase;
		}

		[Priority(10)]
		[Caption("Wydział")]
		[DefaultWidth(15)]
		[Required]
		public ProWydzial Wydzial
		{
			get => _wydzial;
			set
			{
				_wydzial = value;
				OnChanged();
			}
		}
	}
}
