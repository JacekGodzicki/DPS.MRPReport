using DPS.MRPReport.Rows;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator.Models;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.RequisitionDocument.RequisitionDocumentsCreator;
using Soneta.Business;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.UIProviders
{
	[Caption("Tworzenie Zapotrzebowań")]
	public class RequisitionDocumentsCreatorUIProvider : DocumentsCreatorUIProviderBase
	{
		public RequisitionDocumentsCreatorUIProvider(Context context, Date[] dates, APSReportMRPElement[] pozycje) : base(context, dates, pozycje)
		{
		}

		protected override OrderDocumentCreatorModel[] GetTowarModels()
		{
			RequisitionDocumentsCreatorLogic.Params pars = new RequisitionDocumentsCreatorLogic.Params
			{
				ReportMRPElements = _pozycje,
				SelectedDates = _selectedDates
			};

			return new RequisitionDocumentsCreatorLogic(Context, pars).GetTowarModels();
		}
	}
}
