using DPS.MRPReport.Rows;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator.Models;
using Soneta.Business;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.UIProviders
{
	[Caption("Tworzenie Zamówień")]
	public class OrderDocumentsCreatorUIProvider : DocumentsCreatorUIProviderBase
	{
		public OrderDocumentsCreatorUIProvider(Context context, Date[] dates, APSReportMRPElement[] elements) : base(context, dates, elements)
		{
		}

		protected override OrderDocumentCreatorModel[] GetTowarModels()
		{
			OrderDocumentsCreatorLogic.Params pars = new OrderDocumentsCreatorLogic.Params
			{
				ReportMRPElements = _pozycje,
				SelectedDates = _selectedDates
			};

			return new OrderDocumentsCreatorLogic(Context, pars).GetTowarModels();
		}
	}
}
