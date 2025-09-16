using Soneta.Business;
using Soneta.Business.Db;
using System.Collections.Generic;

namespace DPS.MRPReport.Models
{
    public class ProductGroupModel : ContextBase
    {
        private bool _selection;

        public ProductGroupModel(Context context, DictionaryItem dictionaryItem) : base(context)
        {
            DictionaryItem = dictionaryItem;
        }

        public DictionaryItem DictionaryItem { get; }
        public ProductGroupModel Parent { get; set; }
        public List<ProductGroupModel> Children { get; set; }

        public bool Selection
        {
            get => _selection;
            set
            {
                _selection = value;
                SetChildrenSelection(_selection);
                OnChanged();
            }
        }

        public void SetSelection(bool selection)
        {
            _selection = selection;
            SetChildrenSelection(_selection);
		}

        private void SetChildrenSelection(bool selection)
        {
            if (Children is null)
            {
                return;
            }

            foreach (ProductGroupModel child in Children)
            {
                child.SetSelection(selection);
            }
        }
    }
}
