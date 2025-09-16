using DPS.MRPReport.UI.Workers.DocumentsGenerators.Enums;
using Soneta.Business;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.Params
{
	public class DocumentsCreatorParams : ContextBase
	{
		private RowsRangeEnum _rowsRange;

		public DocumentsCreatorParams(Context context) : base(context)
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
