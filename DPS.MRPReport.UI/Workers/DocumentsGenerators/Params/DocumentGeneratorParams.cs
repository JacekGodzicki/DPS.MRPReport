using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Tables.Extensions;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.Enums;
using Soneta.Business;
using Soneta.Magazyny;
using Soneta.Tools;
using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.Params
{
	public class DocumentGeneratorParams : ContextBase
	{
		private Magazyn _magazyn;
		private GroupingTypeEnum _groupingType;

		public DocumentGeneratorParams(Context context) : base(context)
		{
			View viewMagazyny = GetViewMagazyny();
			if(viewMagazyny.Count == 1)
			{
				_magazyn = viewMagazyny.GetFirst() as Magazyn;
			}
		}

		[Priority(10)]
		[Caption("Grupowanie")]
		[DefaultWidth(15)]
		public GroupingTypeEnum GroupingType
		{
			get => _groupingType;
			set
			{
				_groupingType = value;
				OnChanged();
			}
		}

		[Priority(20)]
		[Caption("Magazyn")]
		[DefaultWidth(15)]
		[Required]
		public Magazyn Magazyn
		{
			get => _magazyn;
			set
			{
				_magazyn = value;
				OnChanged();
			}
		}

		public object GetListMagazyn()
		{
			View viewMagazyny = GetViewMagazyny();
			return new LookupInfo(viewMagazyny);
		}

		private View GetViewMagazyny()
		{
			RowCondition rc = new RowCondition.Exists(nameof(APSMagExt),
				nameof(APSMagazynExt.Magazyn),
				new FieldCondition.Equal(nameof(APSMagazynExt.IsReportMRP), true)
			);

			return Session.GetMagazyny().Magazyny.PrimaryKey[rc].CreateView();
		}
	}
}
