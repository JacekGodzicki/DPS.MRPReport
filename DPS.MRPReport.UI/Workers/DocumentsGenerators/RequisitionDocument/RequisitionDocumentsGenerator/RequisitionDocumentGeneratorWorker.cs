using DPS.MRPReport.UI.Workers.DocumentsGenerators.RequisitionDocument.RequisitionDocumentsGenerator;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.UIProviders;
using Soneta.Business;
using Soneta.Handel;
using System;

[assembly: Worker(typeof(RequisitionDocumentGeneratorWorker), typeof(RequisitionDocumentsCreatorUIProvider))]

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.RequisitionDocument.RequisitionDocumentsGenerator
{
	public class RequisitionDocumentGeneratorWorker
	{
		[Context]
		public Session Session { get; set; }

		[Context]
		public Context Context { get; set; }

		[Context]
		public RequisitionDocumentsCreatorUIProvider DocumentsCreatorUIProvider { get; set; }

		[Action("Generuj Zapotrzebowanie",
			Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished
					| ActionMode.Progress | ActionMode.NonCancelProgress,
			Target = ActionTarget.ToolbarWithText,
			Priority = 1,
			Icon = ActionIcon.Wizard)]
		public object Action()
		{
			if(!AreParamsValid(out string message))
			{
				throw new Exception(message);
			}
			return CreateQueryContextInformation();
		}

		private bool AreParamsValid(out string message)
		{
			message = string.Empty;
			return true;
		}

		private QueryContextInformation CreateQueryContextInformation()
		{
			RequisitionDocumentGeneratorParams pars = new RequisitionDocumentGeneratorParams(Context);
			return QueryContextInformation.Create((RequisitionDocumentGeneratorParams pars) =>
			{
				return Create(pars);
			});
		}

		private DokumentHandlowy Create(RequisitionDocumentGeneratorParams pars)
		{
			DocumentsGeneratorLogic.Params logicPars = new DocumentsGeneratorLogic.Params
			{
				DefDokHandlowego = pars.DefDokHandlowego,
				Magazyn = pars.Magazyn,
				Models = DocumentsCreatorUIProvider.SelectedTowarModels,
				RodzajGrupowania = pars.GroupingType
			};

			return new DocumentsGeneratorLogic(Session, logicPars).Create();
		}
	}
}
