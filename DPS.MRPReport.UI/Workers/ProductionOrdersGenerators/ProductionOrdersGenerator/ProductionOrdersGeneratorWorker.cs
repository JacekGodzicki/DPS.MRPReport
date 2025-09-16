using Soneta.Business;
using Soneta.ProdukcjaPro;
using System;
using System.Linq;
using DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersGenerator;
using DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersCreator.Models;
using DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersCreator;
using DPS.MRPReport.Tables;
using DPS.MRPReport.Configurations;

[assembly: SimpleRight(typeof(APSReportMRP), ReportMRPConfiguration.SimpleRights.ProductionOrderCreatorWorker)]
[assembly: Worker(typeof(ProductionOrdersGeneratorWorker), typeof(ProductionOrderCreatorUIProvider))]

namespace DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersGenerator
{
	public class ProductionOrdersGeneratorWorker
	{
		[Context]
		public Session Session { get; set; }

		[Context]
		public ProductionOrderCreatorUIProvider ZleceniaProdukcyjneCreatorUI { get; set; }

		[Action("Generuj Zlecenie produkcyjne",
		  Mode = ActionMode.SingleSession | ActionMode.ConfirmFinished
				| ActionMode.Progress | ActionMode.NonCancelProgress,
		  Target = ActionTarget.ToolbarWithText,
		  Priority = 30,
		  Icon = ActionIcon.Wizard)]
		public object Create()
		{
			TowarModelContextBase model = ZleceniaProdukcyjneCreatorUI.SelectedTowarModels.FirstOrDefault();
            if (model is null)
            {
				return null;
            }

			if(model.Technologia.Wydzial is not null)
			{
				return CreateProZlecenie(model);
			}

			return QueryContextInformation.Create((ProductionOrdersGeneratorParams pars) =>
			{
				return CreateProZlecenie(pars, model);
			});
		}

		private ProZlecenie CreateProZlecenie(TowarModelContextBase model)
		{
			ProductionOrdersGeneratorLogic.Params logicPars = new ProductionOrdersGeneratorLogic.Params
			{
				Model = model,
				Wydzial = model.Technologia.Wydzial
			};
			return new ProductionOrdersGeneratorLogic(Session, logicPars).Create();
		}

		private ProZlecenie CreateProZlecenie(ProductionOrdersGeneratorParams pars, TowarModelContextBase model)
		{
			ProductionOrdersGeneratorLogic.Params logicPars = new ProductionOrdersGeneratorLogic.Params
			{
				Model = model,
				Wydzial = pars.Wydzial
			};
			return new ProductionOrdersGeneratorLogic(Session, logicPars).Create();
		}
	}
}
