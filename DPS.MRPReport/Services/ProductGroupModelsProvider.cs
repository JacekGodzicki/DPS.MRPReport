using DPS.MRPReport.Models;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.Business.Db;
using Soneta.Towary;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Services
{
	public class ProductGroupModelsProvider
	{
		private readonly Context _context;
		private readonly Session _session;

		public ProductGroupModelsProvider(Context context)
		{
			_context = context;
			_session = context.Session;
		}

		public List<ProductGroupModel> GetModels()
		{
			IEnumerable<DictionaryItem> dictionaryItems = GetDictionaryItems();
			List<ProductGroupModel> models = GetAllModels(dictionaryItems);
			FillModelsRelations(models);
			return models
				.Where(x => x.Parent is null)
				.ToList();
		}

		private List<ProductGroupModel> GetAllModels(IEnumerable<DictionaryItem> dictionaryItems)
		{
			List<ProductGroupModel> models = new List<ProductGroupModel>();
			foreach(DictionaryItem dictionaryItem in dictionaryItems)
			{
				ProductGroupModel model = new ProductGroupModel(_context, dictionaryItem);
				models.Add(model);
			}
			return models;
		}

		private void FillModelsRelations(IEnumerable<ProductGroupModel> models)
		{
			foreach(ProductGroupModel model in models)
			{
				if(model.DictionaryItem.Parent is not null)
				{
					model.Parent = models.FirstOrDefault(x => x.DictionaryItem == model.DictionaryItem.Parent);
				}

				if(model.DictionaryItem.Children is not null)
				{
					List<ProductGroupModel> childrenModels = new List<ProductGroupModel>();
					foreach(DictionaryItem child in model.DictionaryItem.Children)
					{
						ProductGroupModel childModel = models.FirstOrDefault(x => x.DictionaryItem == child);
						if(childModel is not null)
						{
							childrenModels.Add(childModel);
						}
					}
					model.Children = childrenModels;
				}
			}
		}

		private IEnumerable<DictionaryItem> GetDictionaryItems()
		{
			FeatureDefinition productGroupFeatureDefinition = GetProductGroupFeatureDefinition();
			if(productGroupFeatureDefinition is null)
			{
				return Enumerable.Empty<DictionaryItem>();
			}

			RowCondition rcDictionaryItems = new FieldCondition.Equal(nameof(DictionaryItem.Category), $"F.{productGroupFeatureDefinition.Dictionary}");
			return _session.GetBusiness().Dictionary.PrimaryKey[rcDictionaryItems]
				.CreateView()
				.Cast<DictionaryItem>()
				.ToList();
		}

		private FeatureDefinition GetProductGroupFeatureDefinition()
		{
			ConfigReportMRPMainWorker configWorker = new ConfigReportMRPMainWorker(_session);
			if(configWorker.FeatureDefinitionCommodityGroup is FeatureDefinition featureDefinition)
			{
				RowCondition rc = new FieldCondition.Equal(nameof(FeatureDefinition.Name), featureDefinition.Name);
				rc &= new FieldCondition.Equal(nameof(FeatureDefinition.TableName), nameof(Towary));
				return _session.GetBusiness().FeatureDefs.PrimaryKey[rc].GetFirst() as FeatureDefinition;
			}
			return null;
		}
	}
}
