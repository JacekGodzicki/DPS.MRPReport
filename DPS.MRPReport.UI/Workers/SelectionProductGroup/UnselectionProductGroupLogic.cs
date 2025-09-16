using Soneta.Business;
using System.Collections.Generic;
using DPS.MRPReport.UI.Workers.SelectionProductGroup.Abstractions;
using DPS.MRPReport.Models;

namespace DPS.MRPReport.UI.Workers.SelectionProductGroup
{
	public class UnselectionProductGroupLogic : SelectionProductGroupLogicBase
	{
		public UnselectionProductGroupLogic(Context context) : base(context, "Usuwanie grup towarowych z raportu MRP")
		{
		}

		protected override void HandleOnCommittingEvent(IEnumerable<ProductGroupModel> models)
		{
			SetTowaryRelatedWithModelsIsReportMRPValue(models, false);
		}
	}
}