using DPS.MRPReport.Models;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.SelectionProductGroup
{
	[Caption("Wybór grup towarowych")]
    public class SelectionProductGroupUI : ContextBase, ICommittable
	{
		private readonly string _name;
		private readonly ProductGroupModel[] _productGroupModels;

		public SelectionProductGroupUI(Context context, string name, IEnumerable<ProductGroupModel> productGroupModels) : base(context)
		{
			_name = name;
			_productGroupModels = productGroupModels.ToArray();
		}

		public event Action<IEnumerable<ProductGroupModel>> OnCommittingEvent;

		public ProductGroupModel[] Source
			=> _productGroupModels;

		public object OnCommitted(Context cx)
		{
			return null;
		}

		public object OnCommitting(Context cx)
		{
			if(OnCommittingEvent is not null)
			{
				List<ProductGroupModel> allModels = GetSelectedModels();
				OnCommittingEvent.Invoke(allModels);
			}
			return null;
		}

		private List<ProductGroupModel> GetSelectedModels()
		{
			List<ProductGroupModel> allModels = new List<ProductGroupModel>();
			foreach(ProductGroupModel model in _productGroupModels)
			{
				if(model.Selection)
				{
					allModels.Add(model);
				}
				allModels.AddRange(GetAllSelectedChildren(model));
			}
			return allModels;
		}

		private List<ProductGroupModel> GetAllSelectedChildren(ProductGroupModel model)
		{
			if(model.Children is null)
			{
				return [];
			}

			List<ProductGroupModel> allModels = new List<ProductGroupModel>();
			foreach(ProductGroupModel child in model.Children)
			{
				if(child.Selection)
				{
					allModels.Add(child);
				}
				allModels.AddRange(GetAllSelectedChildren(child));

			}
			return allModels;
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
