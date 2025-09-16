using DPS.MRPReport.UI.Workers.DocumentsGenerators.Enums;
using Soneta.Business;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator
{
    public class OrderDocumentsCreatorParams : ContextBase
    {
        private RowsRangeEnum _rowsRange;

        public OrderDocumentsCreatorParams(Context context) : base(context)
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
