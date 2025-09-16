using DPS.MRPReport.Enums;
using DPS.MRPReport.Utils;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.ProdukcjaPro;
using Soneta.Tools;
using Soneta.Towary;
using Soneta.Types;
using System;
using System.Linq;

namespace DPS.MRPReport.Rows.Extensions
{
	[Caption("Raport MRP - Towar")]
	public class APSTowarExt : DPSMRPReportModule.APSTowarExtRow
	{
		private bool _onlyFixedMnimalStock = false;
		private bool _onlyFixedMnimalStockInitialized = false;

		public APSTowarExt(RowCreator creator) : base(creator)
		{
		}

		public APSTowarExt([Required] Towar towar) : base(towar)
		{
		}

		[Caption("Grupa")]
		public string CommodityGroup
		{
			get
			{
				ConfigReportMRPMainWorker configWorker = new ConfigReportMRPMainWorker(Session);
				if(configWorker.FeatureDefinitionCommodityGroup is null)
				{
					return string.Empty;
				}

				if((configWorker.FeatureDefinitionCommodityGroup.TypeNumber == FeatureTypeNumber.ArrayOfTrees
					|| configWorker.FeatureDefinitionCommodityGroup.TypeNumber == FeatureTypeNumber.Array)
					&& Towar.Features.GetArray(configWorker.FeatureDefinitionCommodityGroup.Name) is string[] values)
				{
					return values.Join("\\");
				}
				return Towar.Features[configWorker.FeatureDefinitionCommodityGroup.Name]?.ToString() ?? string.Empty;
			}
		}

		[Caption("Domyślna technologia")]
		public ProTechnologia DefaultTechnology
		{
			get
			{
				RowCondition rc = new FieldCondition.Equal(nameof(ProTechnologia.Towar), base.Towar);
				rc &= new FieldCondition.Equal(nameof(ProTechnologia.Domyslna), true);
				return Session.GetProdukcjaPro().ProTechnologie.PrimaryKey[rc].GetFirst() as ProTechnologia;
			}
		}

        [Caption("Posiada domyślną technologię")]
        public bool ExistsDefaultTechnology
        {
            get
            {
                RowCondition rc = new FieldCondition.Equal(nameof(ProTechnologia.Towar), base.Towar);
                rc &= new FieldCondition.Equal(nameof(ProTechnologia.Domyslna), true);
                return Session.GetProdukcjaPro().ProTechnologie.PrimaryKey[rc].Any;
            }
        }

        [AttributeInheritance]
        public override bool MRPIsReportMRP
        {
            get => base.MRPIsReportMRP;
            set
            {
                base.MRPIsReportMRP = value;
                if (!base.MRPIsReportMRP)
                {
                    SetDefaultValues();
                }
            }
        }

        [AttributeInheritance]
        public override Quantity MRPLogisticalMinimum
        {
            get => base.MRPLogisticalMinimum;
            set
            {
                if (value == Quantity.Empty || value.Value < 0)
                {
                    base.MRPLogisticalMinimum = new Quantity(0, Towar.Jednostka.Kod);
                    return;
                }

                base.MRPLogisticalMinimum = new Quantity(value.Value, Towar.Jednostka.Kod);
            }
        }

        [AttributeInheritance]
        public override Quantity MRPMinimumReserve
        {
            get => base.MRPMinimumReserve;
            set
            {
                if (value == Quantity.Empty || value.Value < 0)
                {
                    base.MRPMinimumReserve = new Quantity(0, Towar.Jednostka.Kod);
                    return;
                }

                if (value.IsZero && (base.MRPOrderingPolicy == OrderingPolicyEnum.MinimumReserve || base.MRPOrderingPolicy == OrderingPolicyEnum.Combination))
                {
                    throw new Exception($"Dla wybranej polityki zamawiania '{CaptionAttribute.EnumToString(base.MRPOrderingPolicy)}' nie można ustawić wartości zero.");
                }

                base.MRPMinimumReserve = new Quantity(value.Value, Towar.Jednostka.Kod);
            }
        }

        [AttributeInheritance]
        public override ObtainingMethodEnum MRPObtainingMethod
        {
            get => base.MRPObtainingMethod;
            set
            {
                base.MRPObtainingMethod = value;
                if (base.MRPObtainingMethod == ObtainingMethodEnum.Manufactured && !ExistsDefaultTechnology)
                {
                    throw new Exception($"Dany towar nie może zostać oznaczony jako {CaptionAttribute.EnumToString(ObtainingMethodEnum.Manufactured)} ponieważ nie posiada domyślnej technologii.");
                }
            }
        }

        [AttributeInheritance]
        public override OrderingPolicyEnum MRPOrderingPolicy
        {
            get => base.MRPOrderingPolicy;
            set
            {
                if (!VerifyValueOrderingPolicyValid(value, out string message))
                {
                    throw new Exception(message);
                }
                base.MRPOrderingPolicy = value;
            }
        }

		[AttributeInheritance]
		public override Quantity MRPOrderingQuantity
		{
			get => base.MRPOrderingQuantity;
			set
			{
				if(value == Quantity.Empty || value.Value < 0)
				{
					base.MRPOrderingQuantity = new Quantity(0, Towar.Jednostka.Kod);
					return;
				}

                if (value.IsZero && (base.MRPOrderingPolicy == OrderingPolicyEnum.FixedQuantity || base.MRPOrderingPolicy == OrderingPolicyEnum.Combination))
                {
                    throw new Exception($"Dla wybranej polityki zamawiania '{CaptionAttribute.EnumToString(base.MRPOrderingPolicy)}' nie można ustawić wartości zero.");
                }

                base.MRPOrderingQuantity = new Quantity(value.Value, Towar.Jednostka.Kod);
            }
        }

		private void SetDefaultValues()
		{
			if(Towar.Jednostka is Jednostka jednostka)
			{
				Quantity quantityZero = new Quantity(0, jednostka.Kod);
				base.MRPMinimumReserve = quantityZero;
				base.MRPLogisticalMinimum = quantityZero;
				base.MRPOrderingQuantity = quantityZero;
			}
			else
			{
				base.MRPMinimumReserve = Quantity.Empty;
				base.MRPLogisticalMinimum = Quantity.Empty;
				base.MRPOrderingQuantity = Quantity.Empty;
			}

            base.MRPOrderingPolicy = OrderingPolicyEnum.RequiredQuantityNetto;
            base.MRPObtainingMethod = ObtainingMethodEnum.Purchased;
        }

        private bool VerifyValueOrderingPolicyValid(OrderingPolicyEnum orderingPolicy, out string message)
        {
            message = string.Empty;
            if (orderingPolicy == OrderingPolicyEnum.MinimumReserve && base.MRPMinimumReserve.IsZero)
            {
                message = "Wprowadź w kolumnie 'Zapas minimalny' wartość większą od zera zanim wybierzesz politykę zamawiania 'Zapas minimalny'";
                return false;
            }

            if (orderingPolicy == OrderingPolicyEnum.FixedQuantity && base.MRPOrderingQuantity.IsZero)
            {
                message = "Wprowadź w kolumnie 'Ilość zamawiana' wartość większą od zera zanim wybierzesz politykę zamawiania 'Stała ilość'";
                return false;
            }

            if (orderingPolicy == OrderingPolicyEnum.Combination && (base.MRPMinimumReserve.IsZero || base.MRPOrderingQuantity.IsZero))
            {
                message = "Wprowadź w kolumnie 'Zapas minimalny' oraz 'Ilość zamawiana' wartość większą od zera zanim wybierzesz politykę zamawiania 'Kombinacja'";
                return false;
            }
            return true;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            SetDefaultValues();

            ConfigReportMRPMainWorker configWorker = new ConfigReportMRPMainWorker(Session);
            base.MRPIsReportMRP = configWorker.DefaultCommodityIsReportMRP;
        }

        protected override void OnDeleting()
        {
            new RowExtensionUtil<Towar, APSTowarExt>().DeletingWithoutExtendedObjectNotAllowed(Towar, this);
            base.OnDeleting();
        }

		public void ChangeUnit()
		{
			if(Towar.Jednostka is Jednostka jednostka)
			{
				base.MRPMinimumReserve = new Quantity(base.MRPMinimumReserve.Value, jednostka.Kod);
				base.MRPLogisticalMinimum = new Quantity(base.MRPLogisticalMinimum.Value, jednostka.Kod);
				base.MRPOrderingQuantity = new Quantity(base.MRPOrderingQuantity.Value, jednostka.Kod);
			}
		}

		public Quantity GetMinimalStock(Date date)
		{
			if(!_onlyFixedMnimalStockInitialized)
			{
				_onlyFixedMnimalStockInitialized = true;
				_onlyFixedMnimalStock = !Session.GetDPSMRPReport().APSMinStocks.WgTowar[Towar].Any;
			}

			if(_onlyFixedMnimalStock)
			{
				return base.MRPMinimumReserve;
			}

			RowCondition rc = new FieldCondition.Equal(nameof(APSMinimalStock.Towar), base.Towar);
			rc &= RowCondition.IsIntersected(nameof(APSMinimalStock.Period), FromTo.Day(date));
			if(Session.GetDPSMRPReport().APSMinStocks.PrimaryKey[rc].GetFirst() is APSMinimalStock minimalStock)
			{
				return minimalStock.Quantity;
			}
			return base.MRPMinimumReserve;
		}

        public bool IsOrderingPolicyCombination()
        {
            return base.MRPOrderingPolicy == OrderingPolicyEnum.Combination;
        }

        public bool IsOrderingPolicyFixedQuantity()
        {
            return base.MRPOrderingPolicy == OrderingPolicyEnum.FixedQuantity;
        }

        public bool IsOrderingPolicyMinimumReserve()
        {
            return base.MRPOrderingPolicy == OrderingPolicyEnum.MinimumReserve;
        }

		public override bool IsReadOnly()
			=> Towar.IsReadOnly();

		public bool IsReadOnlyMRPLogisticalMinimum()
		{
			return !MRPIsReportMRP
				|| !new ConfigReportMRPMainWorker(Session).IncludeOrderingPolicy;
		}

        public bool IsReadOnlyMRPObtainingMethod()
        {
            return !MRPIsReportMRP;
        }

        public bool IsReadOnlyMRPObtainingPeriod()
        {
            return !MRPIsReportMRP;
        }

        public bool IsReadOnlyMRPOrderingPolicy()
        {
            return !MRPIsReportMRP
                || !new ConfigReportMRPMainWorker(Session).IncludeOrderingPolicy;
        }

        public bool IsReadOnlyMRPOrderingQuantity()
        {
            return !MRPIsReportMRP
                || !new ConfigReportMRPMainWorker(Session).IncludeOrderingPolicy;
        }

        public void SetMRPObtainingMethodBasedOnTowarTyp()
        {
            using (ITransaction transaction = Session.Logout(true))
            {
                if (base.Towar.Typ == TypTowaru.Produkt)
                {
                    base.MRPObtainingMethod = ObtainingMethodEnum.Manufactured;
                }
                else
                {
                    base.MRPObtainingMethod = ObtainingMethodEnum.Purchased;
                }

                transaction.Commit();
            }
        }

		public override string ToString()
		{
			return Towar.ToString();
		}
	}
}