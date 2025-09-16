using DPS.MRPReport.Enums;
using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Tables.Extensions;
using DPS.MRPReport.Workers.Config.Abstractions;
using Soneta.Business;
using Soneta.Business.Db;
using Soneta.Config;
using Soneta.Handel;
using Soneta.Magazyny;
using Soneta.Towary;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DPS.MRPReport.Workers.Config
{
	public class ConfigReportMRPMainWorker : ConfigurationNodesManager
    {
        public ConfigReportMRPMainWorker(Session session)
        {
            Session = session;
        }

        public override string MinorNode => "ConfigReportMRPOgolne";

		[Category("Ogólne")]
		[Caption("Dokładność wyliczeń")]
        public CalculationsAccuracyEnum CalculationsAccuracy
		{
            get => GetValue<CalculationsAccuracyEnum>(nameof(CalculationsAccuracy), (int)CalculationsAccuracyEnum.Days);
            set => SetValue(nameof(CalculationsAccuracy), (int)value, AttributeType._int);
        }

		[Category("Ogólne")]
		[Caption("Uwzględniać wyłącznie dni robocze")]
		public bool IncludeOnlyWorkDays
        {
			get => GetValue(nameof(IncludeOnlyWorkDays), true);
			set => SetValue(nameof(IncludeOnlyWorkDays), value, AttributeType._boolean);
		}

		[Category("Ogólne")]
		[Caption("Ograniczać liczbę dni przeliczanych wprzód")]
		public bool LimitNumberOfDaysAheadForCalc
        {
			get => GetValue(nameof(LimitNumberOfDaysAheadForCalc), false);
            set
            {
				SetValue(nameof(LimitNumberOfDaysAheadForCalc), value, AttributeType._boolean);
				if(value)
                {
					SetValue(nameof(NumberOfDaysAheadForCalc), 1, AttributeType._int);
				}
                else
                {
					SetValue(nameof(NumberOfDaysAheadForCalc), 0, AttributeType._int);
				}
			}
		}

		[Category("Ogólne")]
		[Caption("Liczba dni przeliczanych wprzód")]
		public int NumberOfDaysAheadForCalc
        {
			get => GetValue(nameof(NumberOfDaysAheadForCalc), 30);
            set
            {
                if(value < 0)
                {
                    throw new ArgumentException("Wartość nie może być mniejsza od 1.");
                }
                SetValue(nameof(NumberOfDaysAheadForCalc), value, AttributeType._int);
            }
		}

		[Category("Ogólne")]
		[Caption("Czy uwzględniać politykę zamawiania")]
		public bool IncludeOrderingPolicy
		{
			get => GetValue(nameof(IncludeOrderingPolicy), false);
			set => SetValue(nameof(IncludeOrderingPolicy), value, AttributeType._boolean);
		}

		[Category("Ogólne")]
		[Caption("Czy zbiorcze sugestie przeterminowane")]
		public bool CollectiveSuggOverdue
		{
			get => GetValue(nameof(CollectiveSuggOverdue), false);
			set => SetValue(nameof(CollectiveSuggOverdue), value, AttributeType._boolean);
		}

		[Category("Ogólne")]
		[Caption("Czy uwzględniać zamienniki")]
		public bool IncludeReplacements
		{
			get => GetValue(nameof(IncludeReplacements), false);
			set => SetValue(nameof(IncludeReplacements), value, AttributeType._boolean);
		}

		[Category("Ogólne")]
		[Caption("Przeliczaj raport po wygenerowaniu zlecenia lub dokumentu")]
		public bool RecalculateAfterCreatingDocumentOrOrder
		{
			get => GetValue(nameof(RecalculateAfterCreatingDocumentOrOrder), false);
			set => SetValue(nameof(RecalculateAfterCreatingDocumentOrOrder), value, AttributeType._boolean);
		}

		[Category("Ogólne")]
		[Caption("Uwzględniaj nowododane towary w raporcie MRP")]
		public bool DefaultCommodityIsReportMRP
		{
			get => GetValue(nameof(DefaultCommodityIsReportMRP), true);
			set => SetValue(nameof(DefaultCommodityIsReportMRP), value, AttributeType._boolean);
		}

		[Category("Ogólne")]
		[Caption("Grupa towarów - definicja cechy")]
		public FeatureDefinition FeatureDefinitionCommodityGroup
		{
			get => GetGuidedRowValue<FeatureDefinition>(Session.GetBusiness().FeatureDefs, nameof(FeatureDefinitionCommodityGroup));
			set => SetGuidedRowValue(nameof(FeatureDefinitionCommodityGroup), value);
		}

		[Category("Ogólne")]
		[Caption("Pomniejszaj plany sprzedaży o zamówienia")]
		public bool ReduceSalesPlansByOrderQuantity
		{
			get => GetValue(nameof(ReduceSalesPlansByOrderQuantity), true);
			set => SetValue(nameof(ReduceSalesPlansByOrderQuantity), value, AttributeType._boolean);
		}


		[Category("Dokumenty")]
		[Caption("Definicje zamówień od odbiorców")]
        public DefDokHandlowego[] OrdersToRecipientsDefs
		{
			get => GetSelectedOrdersToRecipientsDefs();
            set
            {
                IEnumerable<DefDokHandlowego> defsDokHandlowego = GetSelectedOrdersToRecipientsDefs();
				SetIsReportMRPValue(defsDokHandlowego, false);
				if (value is null || !value.Any())
                {
                    return;
                }

				SetIsReportMRPValue(value, true);
			}
        }

		[Category("Dokumenty")]
		[Caption("Uwzględniać korekty zamówień od odbiorców")]
		public bool IncludeOrdersToRecipientsCorrections
        {
			get => GetValue(nameof(IncludeOrdersToRecipientsCorrections), true);
			set => SetValue(nameof(IncludeOrdersToRecipientsCorrections), value, AttributeType._boolean);
		}

		[Category("Dokumenty")]
		[Caption("Definicje zamówień do dostawców")]
        public DefDokHandlowego[] OrdersToSupplierDefs
		{
            get => GetSelectedOrdersToSupplierDefs();
            set
            {
                IEnumerable<DefDokHandlowego> defsDokHandlowego = GetSelectedOrdersToSupplierDefs();
				SetIsReportMRPValue(defsDokHandlowego, false);

				if (value is null || !value.Any())
                {
                    return;
                }

				SetIsReportMRPValue(value, true);
            }
        }

		[Category("Dokumenty")]
		[Caption("Uwzględniać korekty zamówień od dostawców")]
		public bool IncludeOrdersToSupplierCorrections
		{
			get => GetValue(nameof(IncludeOrdersToSupplierCorrections), true);
			set => SetValue(nameof(IncludeOrdersToSupplierCorrections), value, AttributeType._boolean);
		}

		[Category("Dokumenty")]
		[Caption("Definicje dokumentów zapotrzebowania")]
		public DefDokHandlowego[] RequisitionDocumentDefs
		{
			get => GetSelectedRequisitionDocumentDefs();
			set
			{
				IEnumerable<DefDokHandlowego> selectedRequisitionDocumentDefs = GetSelectedRequisitionDocumentDefs();
				SetMRPIsRequisitionDefValue(selectedRequisitionDocumentDefs, false);
				if(value is null || !value.Any())
				{
					return;
				}

				SetMRPIsRequisitionDefValue(value, true);
			}
		}

		[Category("Dokumenty")]
		[Caption("Czy uwzględniać dokumenty w buforze")]
        public bool IncludeDocsInBuffer
        {
            get => GetValue(nameof(IncludeDocsInBuffer), false);
            set
            {
				ApproveDocuments = true;
                SetValue(nameof(IncludeDocsInBuffer), value, AttributeType._boolean);
            }
        }

		[Category("Dokumenty")]
		[Caption("Czy uwzględniać dokumenty zapotrzebowania")]
		public bool IncludeRequisitionDocuments
		{
			get => GetValue(nameof(IncludeRequisitionDocuments), false);
			set => SetValue(nameof(IncludeRequisitionDocuments), value, AttributeType._boolean);
		}

		[Category("Dokumenty")]
		[Caption("Czy zatwierdzać dokumenty przy generacji z sugestii")]
        public bool ApproveDocuments
        {
            get => GetValue(nameof(ApproveDocuments), false);
            set => SetValue(nameof(ApproveDocuments), value, AttributeType._boolean);
        }

		[Category("Akcje (Workery)")]
		[Caption("Czy można generować ZP z SZ")]
		public bool AllowGenerateZPFromSZ
		{
			get => GetValue(nameof(AllowGenerateZPFromSZ), true);
			set => SetValue(nameof(AllowGenerateZPFromSZ), value, AttributeType._boolean);
		}

		[Category("Akcje (Workery)")]
		[Caption("Czy można generować ZD z SP")]
		public bool AllowGenerateZDFromSP
		{
			get => GetValue(nameof(AllowGenerateZDFromSP), true);
			set => SetValue(nameof(AllowGenerateZDFromSP), value, AttributeType._boolean);
		}

		[Category("Akcje (Workery)")]
		[Caption("Czy zmiana SP na SZ usuwa materiały")]
		public bool ChangeSPToSZRemoveMaterials
		{
			get => GetValue(nameof(ChangeSPToSZRemoveMaterials), false);
			set => SetValue(nameof(ChangeSPToSZRemoveMaterials), value, AttributeType._boolean);
		}

		[Category("Generowanie zamówień")]
		[Caption("Ograniczaj kontrahentów do dostawców towaru")]
		public bool OrdersGenLimitContractorsToSuppliers
		{
			get => GetValue(nameof(OrdersGenLimitContractorsToSuppliers), false);
			set => SetValue(nameof(OrdersGenLimitContractorsToSuppliers), value, AttributeType._boolean);
		}

		public bool IsVisibleNumberOfDaysAheadForCalc()
        {
            return LimitNumberOfDaysAheadForCalc;
        }

		public bool IsVisibleApproveDocuments()
        {
            return IncludeDocsInBuffer;
        }

		public object GetListFeatureDefinitionCommodityGroup()
        {
            RowCondition rc = new FieldCondition.Equal(nameof(FeatureDefinition.TableName), nameof(Towary));
			View view = Session.GetBusiness().FeatureDefs.PrimaryKey[rc].CreateView();
			return new LookupInfo(view);
		}

		public object GetListOrdersToRecipientsDefs()
        {
            RowCondition rc = new FieldCondition.Equal(nameof(DefDokHandlowego.Kategoria), KategoriaHandlowa.ZamówienieOdbiorcy);
            rc &= new FieldCondition.Equal(nameof(DefDokHandlowego.Blokada), false);

			DefDokHandlowego[] defs = Session.GetHandel().DefDokHandlowych.PrimaryKey[rc]
                .Cast<DefDokHandlowego>()
                .Where(x => !x.DefinicjaKorekty)
                .ToArray();

            View view = new View(Session.GetHandel().DefDokHandlowych.PrimaryKey, defs);
            return new LookupInfo(view);
        }

        public object GetListOrdersToSupplierDefs()
        {
            RowCondition rc = new FieldCondition.Equal(nameof(DefDokHandlowego.Kategoria), KategoriaHandlowa.ZamówienieDostawcy);
            rc &= new FieldCondition.Equal(nameof(DefDokHandlowego.Blokada), false);

			DefDokHandlowego[] defs = Session.GetHandel().DefDokHandlowych.PrimaryKey[rc]
				.Cast<DefDokHandlowego>()
				.Where(x => !x.DefinicjaKorekty)
				.ToArray();

			View view = new View(Session.GetHandel().DefDokHandlowych.PrimaryKey, defs);
			return new LookupInfo(view);
        }

		public object GetListRequisitionDocumentDefs()
		{
			RowCondition rc = new RowCondition.Not(
                new FieldCondition.In(nameof(DefDokHandlowego.Kategoria), [KategoriaHandlowa.ZamówienieDostawcy, KategoriaHandlowa.ZamówienieOdbiorcy])
            );
			rc &= new FieldCondition.Equal(nameof(DefDokHandlowego.Blokada), false);
			rc &= new FieldCondition.Equal(nameof(DefDokHandlowego.KierunekMagazynu), KierunekPartii.Przychód);

			DefDokHandlowego[] defs = Session.GetHandel().DefDokHandlowych.PrimaryKey[rc]
				.Cast<DefDokHandlowego>()
				.Where(x => !x.DefinicjaKorekty)
				.ToArray();

			View view = new View(Session.GetHandel().DefDokHandlowych.PrimaryKey, defs);
			return new LookupInfo(view);
		}

		private void SetIsReportMRPValue(IEnumerable<DefDokHandlowego> defsDokHandlowego, bool isReportMRP)
        {
            using (ITransaction transaction = Session.Logout(true))
            {
				foreach(DefDokHandlowego defDokHandlowego in defsDokHandlowego)
				{
					defDokHandlowego.GetAPSExt().IsReportMRP = isReportMRP;
				}

				transaction.CommitUI();
            }
        }

        private void SetMRPIsRequisitionDefValue(IEnumerable<DefDokHandlowego> defsDokHandlowego, bool mrpIsRequisitionDef)
        {
			using(ITransaction transaction = Session.Logout(true))
			{
				foreach(DefDokHandlowego defDokHandlowego in defsDokHandlowego)
				{
					defDokHandlowego.GetAPSExt().MRPIsRequisitionDef = mrpIsRequisitionDef;
				}

				transaction.CommitUI();
			}
		}

		private DefDokHandlowego[] GetSelectedOrdersToRecipientsDefs()
        {
			RowCondition rc = new RowCondition.Exists(nameof(APSDefDHExt),
			nameof(APSDefDokHandlowegoExt.DefDokHandlowego),
			new RowCondition.And(
				new FieldCondition.Equal(nameof(APSDefDokHandlowegoExt.IsReportMRP), true),
				new FieldCondition.Equal($"{nameof(APSDefDokHandlowegoExt.DefDokHandlowego)}.{nameof(APSDefDokHandlowegoExt.DefDokHandlowego.Blokada)}", false),
				new FieldCondition.Equal($"{nameof(APSDefDokHandlowegoExt.DefDokHandlowego)}.{nameof(APSDefDokHandlowegoExt.DefDokHandlowego.Kategoria)}", KategoriaHandlowa.ZamówienieOdbiorcy)
			)
		);

			return Session.GetHandel().DefDokHandlowych.PrimaryKey[rc]
				.CreateView()
				.Cast<DefDokHandlowego>()
				.ToArray();
		}

        private DefDokHandlowego[] GetSelectedOrdersToSupplierDefs()
        {
			RowCondition rc = new RowCondition.Exists(nameof(APSDefDHExt),
				nameof(APSDefDokHandlowegoExt.DefDokHandlowego),
				new RowCondition.And(
					new FieldCondition.Equal(nameof(APSDefDokHandlowegoExt.IsReportMRP), true),
					new FieldCondition.Equal($"{nameof(APSDefDokHandlowegoExt.DefDokHandlowego)}.{nameof(APSDefDokHandlowegoExt.DefDokHandlowego.Blokada)}", false),
					new FieldCondition.Equal($"{nameof(APSDefDokHandlowegoExt.DefDokHandlowego)}.{nameof(APSDefDokHandlowegoExt.DefDokHandlowego.Kategoria)}", KategoriaHandlowa.ZamówienieDostawcy)
				)
			);

			return Session.GetHandel().DefDokHandlowych.PrimaryKey[rc]
				.CreateView()
				.Cast<DefDokHandlowego>()
				.ToArray();
		}

		private DefDokHandlowego[] GetSelectedRequisitionDocumentDefs()
		{
			RowCondition rc = new RowCondition.Exists(nameof(APSDefDHExt),
				nameof(APSDefDokHandlowegoExt.DefDokHandlowego),
				new RowCondition.And(
					new FieldCondition.Equal(nameof(APSDefDokHandlowegoExt.MRPIsRequisitionDef), true),
					new FieldCondition.Equal($"{nameof(APSDefDokHandlowegoExt.DefDokHandlowego)}.{nameof(APSDefDokHandlowegoExt.DefDokHandlowego.Blokada)}", false)
				)
			);

			return Session.GetHandel().DefDokHandlowych.PrimaryKey[rc]
				.CreateView()
				.Cast<DefDokHandlowego>()
				.ToArray();
		}
	}
}
