using Soneta.Business;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersCreator
{
	public class ProductionOrderCreatorParams : ContextBase
	{
		public enum RowsRangeEnum
		{
			[Caption("Zaznaczone")]
			Selected,
			[Caption("Wszystkie")]
			All
		}

		private RowsRangeEnum _rowsRange;

		public ProductionOrderCreatorParams(Context context) : base(context)
		{
		}

		[Caption("Zakres")]
		[DefaultWidth(15)]
		public RowsRangeEnum RowsRange
		{
			get => _rowsRange;
			set
			{
				_rowsRange = value;
				OnChanged();
			}
		}
	}
}
