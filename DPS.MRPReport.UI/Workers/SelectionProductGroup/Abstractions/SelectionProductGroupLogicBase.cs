using DPS.MRPReport.Models;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Services;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.SelectionProductGroup.Abstractions
{
	public abstract class SelectionProductGroupLogicBase
	{
		private readonly string _pageName;
		protected readonly Context _context;
		protected readonly Session _session;

		public SelectionProductGroupLogicBase(Context context, string pageName)
		{
			_context = context;
			_pageName = pageName;
			_session = context.Session;
		}

		private IEnumerable<APSTowarExt> GetAPSTowarExtsForModel(ProductGroupModel model)
		{
			ConfigReportMRPMainWorker configWorker = new ConfigReportMRPMainWorker(_session);
			if(configWorker.FeatureDefinitionCommodityGroup?.Name is string featureName)
			{
				RowCondition rc = new RowCondition.Or(
					new FieldCondition.Equal($"{nameof(APSTowarExt.Towar)}.{nameof(APSTowarExt.Towar.Features)}.{featureName}", $"\\{model.DictionaryItem.Path}"),
					new FieldCondition.Equal($"{nameof(APSTowarExt.Towar)}.{nameof(APSTowarExt.Towar.Features)}.{featureName}", $"\\{model.DictionaryItem.Path}\\")
				);
				return _session.GetDPSMRPReport().APSTowaryExt.PrimaryKey[rc]
					.Cast<APSTowarExt>();
			}
			return [];
		}

		private List<ProductGroupModel> GetProductGroupModels()
		{
			ProductGroupModelsProvider provider = new ProductGroupModelsProvider(_context);
			return provider.GetModels();
		}

		protected abstract void HandleOnCommittingEvent(IEnumerable<ProductGroupModel> models);

		protected void SetTowaryRelatedWithModelsIsReportMRPValue(IEnumerable<ProductGroupModel> models, bool value)
		{
			using(ITransaction transaction = _session.Logout(true))
			{
				foreach(ProductGroupModel model in models)
				{
					IEnumerable<APSTowarExt> towaryExts = GetAPSTowarExtsForModel(model);
					foreach(APSTowarExt towarExt in towaryExts)
					{
						towarExt.MRPIsReportMRP = value;
					}
				}
				transaction.CommitUI();
			}
		}

		public SelectionProductGroupUI GetUI()
		{
			List<ProductGroupModel> models = GetProductGroupModels();
			SelectionProductGroupUI workerUI = new SelectionProductGroupUI(_context, _pageName, models);
			workerUI.OnCommittingEvent += HandleOnCommittingEvent;
			return workerUI;
		}
	}
}