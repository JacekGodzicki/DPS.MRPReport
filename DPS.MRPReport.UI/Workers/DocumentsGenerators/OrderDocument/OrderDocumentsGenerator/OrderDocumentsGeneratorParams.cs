using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Tables.Extensions;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.Enums;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.Handel;
using Soneta.Magazyny;
using Soneta.Tools;
using Soneta.Types;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsGenerator
{
	public class OrderDocumentsGeneratorParams : ContextBase
	{
		private DefDokHandlowego _defDokHandlowego;
		private GroupingTypeEnum _groupingType;
		private Magazyn _magazyn;

		public OrderDocumentsGeneratorParams(Context context) : base(context)
		{
			Initialize();
		}

		[Priority(10)]
		[Caption("Definicja dokumentu")]
		[DefaultWidth(15)]
		[Required]
		public DefDokHandlowego DefDokHandlowego
		{
			get => _defDokHandlowego;
			set
			{
				_defDokHandlowego = value;
				OnChanged();
			}
		}

		[Priority(20)]
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

		[Priority(30)]
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

		private void Initialize()
		{
			View viewMagazyny = GetViewMagazyny();
			if(viewMagazyny.Count == 1)
			{
				_magazyn = viewMagazyny.GetFirst() as Magazyn;
			}

			ConfigReportMRPMainWorker configWorker = new ConfigReportMRPMainWorker(Session);
			if(configWorker.OrdersToSupplierDefs is DefDokHandlowego[] defs)
			{
				_defDokHandlowego = defs.FirstOrDefault();
			}
		}

		public LookupInfo GetListDefDokHandlowego()
		{
			ConfigReportMRPMainWorker configWorker = new ConfigReportMRPMainWorker(Session);
			DefDokHandlowego[] defs = configWorker.OrdersToSupplierDefs ?? [];
			View viewDefDokHandlowego = new View(Session.GetHandel().DefDokHandlowych.PrimaryKey, defs);
			return new LookupInfo(viewDefDokHandlowego);
		}

		public LookupInfo GetListMagazyn()
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