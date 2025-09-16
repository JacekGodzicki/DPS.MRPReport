using Soneta.Business;
using Soneta.CRM;
using Soneta.Towary;
using Soneta.Types;
using System.Linq;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator.Models;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Utils;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.UIProviders
{
	public abstract class DocumentsCreatorUIProviderBase : ContextBase
	{
		private OrderDocumentCreatorModel[] _selectedTowarModels;
		private OrderDocumentCreatorModel[] _towarModels;
		protected readonly APSReportMRPElement[] _pozycje;
		protected readonly Date[] _selectedDates;

		public DocumentsCreatorUIProviderBase(Context context, Date[] dates, APSReportMRPElement[] pozycje) : base(context)
		{
			_selectedDates = dates;
			_pozycje = pozycje;
		}

		public OrderDocumentCreatorModel[] SelectedTowarModels
		{
			get => _selectedTowarModels;
			set
			{
				_selectedTowarModels = value;
			}
		}

		public OrderDocumentCreatorModel[] TowarModels
		{
			get
			{
				if(_towarModels is null)
				{
					_towarModels = GetTowarModels();
				}

				return _towarModels
					.Where(x => !x.ExistsOrder)
					.ToArray();
			}
		}

		protected abstract OrderDocumentCreatorModel[] GetTowarModels();
	}
}