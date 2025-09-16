using DPS.MRPReport.Models;
using DPS.MRPReport.UI.Workers.SelectionProductGroup.Abstractions;
using Soneta.Business;
using System.Collections.Generic;

namespace DPS.MRPReport.UI.Workers.SelectionProductGroup
{
	public class SelectionProductGroupLogic : SelectionProductGroupLogicBase
	{
		public SelectionProductGroupLogic(Context context) : base(context, "Dodawanie grup towarowych do raportu MRP")
		{
		}

		protected override void HandleOnCommittingEvent(IEnumerable<ProductGroupModel> models)
		{
			SetTowaryRelatedWithModelsIsReportMRPValue(models, true);
		}
	}
}
