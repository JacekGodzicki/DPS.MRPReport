using DPS.MRPReport.Configurations;
using DPS.MRPReport.Enums;
using DPS.MRPReport.Extensions;
using Soneta.Business;
using Soneta.Magazyny;
using Soneta.Towary;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.Rows
{
	[Caption("Pozycja raportu MRP")]
	public class APSReportMRPPosition : DPSMRPReportModule.APSReportMRPPositionRow
	{
		public APSReportMRPPosition(RowCreator creator) : base(creator)
		{
		}

		public APSReportMRPPosition([Required] APSReportMRPElement reportMRPElement) : base(reportMRPElement)
		{
			base.baseObtainingMethod = reportMRPElement.ObtainingMethod;
		}

		[Caption("Ilość przychód")]
		public Quantity IcomeQuantity
		{
			get
			{
				if(Direction == KierunekPartii.Przychód)
				{
					return Quantity;
				}
				return Quantity.Empty;
			}
		}

		[Caption("Ilość rozchód")]
		public Quantity OutcomeQuantity
		{
			get
			{
				if(Direction == KierunekPartii.Rozchód)
				{
					return Quantity;
				}
				return Quantity.Empty;
			}
		}

		[Caption("Pozycja nadrzędna do sugestii")]
		public APSReportMRPPosition RelatedSuggestionParent
			=> GetRelatedSuggestionParentRel()?.Parent;

		[Caption("Relacja nadrzędna do sugestii")]
		public APSReportMRPPositionRel RelatedSuggestionParentRel
			=> GetRelatedSuggestionParentRel();

		[Caption("Powiązana sugestia")]
		public APSReportMRPPosition RelatedSuggestionChild
			=> GetRelatedSuggestionChild();

		[Caption("Nadrzędna pozycja")]
		public APSReportMRPPosition Parent
			=> GetParent();

		[Caption("Podrzędne pozycje (lista)")]
		public List<APSReportMRPPosition> Children
			=> GetChildren();

		[Caption("Podrzędne pozycje")]
		public View ChildrenView
			=> GetChildrenView();

		[Caption("Podrzędne relacje")]
		public List<APSReportMRPPositionRel> ChildrenRelations
			=> GetChildrenRelations();

		public bool IsVisibleChildrenView()
		{
			return ChildrenView.Count > 0;
		}

		public bool IsSuggestion()
		{
			return IsProductionSuggestion() || IsPurchaseSuggestion();
		}

		public bool IsProductionSuggestion()
		{
			return DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.SP;

		}

		public bool IsPurchaseSuggestion()
		{
			return DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.SZ;
		}

		public void SetRelatedSuggestion(APSReportMRPPosition position)
		{
			if(position is not null && !position.IsSuggestion())
			{
				throw new ArgumentException($"Pozycja '{position}' nie jest sugetią.");
			}

			APSReportMRPPosition relatedSuggestion = GetRelatedSuggestionChild();
			if(relatedSuggestion is not null)
			{
				using(ITransaction transaction = Session.Logout(true))
				{
					relatedSuggestion.Delete();

					transaction.CommitUI();
				}
			}

			if(position is null)
			{
				return;
			}

			using(ITransaction transaction = Session.Logout(true))
			{
				APSReportMRPPositionRel positionRel = new APSReportMRPPositionRel(this, position, APSReportMRPPositionRelTypeEnum.Suggestion);
				Session.AddRow(positionRel);

				transaction.CommitUI();
			}
		}

		public void SetParent(APSReportMRPPosition position)
		{
			APSReportMRPPosition parent = GetParent();
			if(parent is not null)
			{
				using(ITransaction transaction = Session.Logout(true))
				{
					parent.Delete();

					transaction.CommitUI();
				}
			}

			if(position is null)
			{
				return;
			}

			using(ITransaction transaction = Session.Logout(true))
			{
				APSReportMRPPositionRel positionRel = new APSReportMRPPositionRel(position, this, APSReportMRPPositionRelTypeEnum.ParentAndChild);
				Session.AddRow(positionRel);

				transaction.CommitUI();
			}
		}

		private APSReportMRPPositionRel GetRelatedSuggestionParentRel()
		{
			DPSMRPReportModule DPSMRPReportModule = Session.GetDPSMRPReport();
			APSReportMRPPositionRel positionRel = DPSMRPReportModule.APSRepMRPPosRel.Rows.Changed
				.Cast<APSReportMRPPositionRel>()
				.FirstOrDefault(x => x.Child == this
								&& x.RelationType == APSReportMRPPositionRelTypeEnum.Suggestion);

			if(positionRel is not null)
			{
				if(positionRel.IsDeletedOrDetached())
				{
					return null;
				}
				return positionRel;
			}

			RowCondition rc = new FieldCondition.Equal(nameof(APSReportMRPPositionRel.Child), this);
			rc &= new FieldCondition.Equal(nameof(APSReportMRPPositionRel.RelationType), APSReportMRPPositionRelTypeEnum.Suggestion);
			return Session.GetDPSMRPReport().APSRepMRPPosRel.PrimaryKey[rc].GetFirst() as APSReportMRPPositionRel;
		}

		private APSReportMRPPosition GetRelatedSuggestionChild()
		{
			DPSMRPReportModule DPSMRPReportModule = Session.GetDPSMRPReport();
			APSReportMRPPositionRel positionRel = DPSMRPReportModule.APSRepMRPPosRel.Rows.Changed
				.Cast<APSReportMRPPositionRel>()
				.FirstOrDefault(x => x.Parent == this
								&& x.RelationType == APSReportMRPPositionRelTypeEnum.Suggestion);

			if(positionRel is not null)
			{
				if(positionRel.IsDeletedOrDetached())
				{
					return null;
				}
				return positionRel.Child;
			}

			RowCondition rc = new FieldCondition.Equal(nameof(APSReportMRPPositionRel.Parent), this);
			rc &= new FieldCondition.Equal(nameof(APSReportMRPPositionRel.RelationType), APSReportMRPPositionRelTypeEnum.Suggestion);
			positionRel = Session.GetDPSMRPReport().APSRepMRPPosRel.PrimaryKey[rc].GetFirst() as APSReportMRPPositionRel;
			return positionRel?.Child;
		}


		private APSReportMRPPosition GetParent()
		{
			DPSMRPReportModule DPSMRPReportModule = Session.GetDPSMRPReport();
			APSReportMRPPositionRel positionRel = DPSMRPReportModule.APSRepMRPPosRel.Rows.Changed
				.Cast<APSReportMRPPositionRel>()
				.FirstOrDefault(x => x.Child == this
								&& x.RelationType == APSReportMRPPositionRelTypeEnum.ParentAndChild);

			if(positionRel is not null)
			{
				if(positionRel.IsDeletedOrDetached())
				{
					return null;
				}
				return positionRel.Parent;
			}

			RowCondition rc = new FieldCondition.Equal(nameof(APSReportMRPPositionRel.Child), this);
			rc &= new FieldCondition.Equal(nameof(APSReportMRPPositionRel.RelationType), APSReportMRPPositionRelTypeEnum.ParentAndChild);
			positionRel = Session.GetDPSMRPReport().APSRepMRPPosRel.PrimaryKey[rc].GetFirst() as APSReportMRPPositionRel;
			return positionRel?.Parent;
		}

		private View GetChildrenView()
		{
			List<APSReportMRPPosition> positions = GetChildren();
			return new View(Session.GetDPSMRPReport().APSReportMRP.PrimaryKey, positions);
		}

		private List<APSReportMRPPosition> GetChildren()
		{
			return GetChildrenRelations()
				.Select(x => x.Child)
				.ToList();
		}

		private List<APSReportMRPPositionRel> GetChildrenRelations()
		{
			RowCondition rc = new FieldCondition.Equal(nameof(APSReportMRPPositionRel.Parent), this);
			rc &= new FieldCondition.Equal(nameof(APSReportMRPPositionRel.RelationType), APSReportMRPPositionRelTypeEnum.ParentAndChild);
			return Session.GetDPSMRPReport().APSRepMRPPosRel.PrimaryKey[rc]
				.CreateView()
				.Cast<APSReportMRPPositionRel>()
				.ToList();
		}

		public override string ToString()
		{
			return $"{base.ReportMRPElement?.Towar} - {base.StartDate} --> {base.AvailabilityDate} - {base.DefinitionCode}";
		}
	}
}