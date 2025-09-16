using DPS.MRPReport.Configurations;
using DPS.MRPReport.Extensions;
using DPS.MRPReport.Models;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Tables;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.Towary;
using Soneta.Types;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.ReportMRP
{
	public class ReportMRPLogic
    {
        private readonly Session _session;
        private readonly Date[] _dates;
        private readonly ReportMRPUIParams _parametry;
        private readonly ConfigReportMRPMainWorker _configRapMRPOgolneWorker;

        public ReportMRPLogic(Session session, Date[] dates, ReportMRPUIParams parametry)
        {
            _session = session;
            _dates = dates;
            _parametry = parametry;
            _configRapMRPOgolneWorker = new ConfigReportMRPMainWorker(session);
        }

        public APSReportMRPElement[] GetCalculatedReportMRPElements()
        {
            if(!_dates.Any())
            {
                return [];
            }

            List<APSReportMRPElement> reportMRPElements = GetReportMRPElements();
            foreach (APSReportMRPElement reportElement in reportMRPElements)
            {
                CalculateReportMRPElementValues(reportElement);
			}
            return reportMRPElements.ToArray();
        }

        private void CalculateReportMRPElementValues(APSReportMRPElement reportElement)
        {
			APSTowarExt towarExt = reportElement.Towar.GetAPSExt();
			List<APSReportMRPPosition> reportMRPPositions = GetReportMRPElementPositionsSorted(reportElement);
			List<APSReportMRPPosition> reportMRPPSuggestionPositions = GetReportMRPSuggestionPositions(reportMRPPositions);

			reportElement.QuantityDates = GetQuantityDates(reportMRPPSuggestionPositions, towarExt.MRPObtainingPeriod);

			APSReportMRPPosition lastReportPosition = reportMRPPositions.LastOrDefault();
			if(lastReportPosition is not null)
			{
				reportElement.FinalBalance = lastReportPosition.BalanceQuantity;

				if(_configRapMRPOgolneWorker.IncludeOrderingPolicy
					&& (towarExt.IsOrderingPolicyMinimumReserve() || towarExt.IsOrderingPolicyCombination()))
				{
					reportElement.Overcapacity = lastReportPosition.BalanceQuantity - towarExt.GetMinimalStock(lastReportPosition.AvailabilityDate);
				}
				else
				{
					reportElement.Overcapacity = lastReportPosition.BalanceQuantity;
				}
			}
			else
			{
				reportElement.FinalBalance = Quantity.Empty;
				reportElement.Overcapacity = Quantity.Empty;
			}
		}

		private QuantityDateModel[] GetQuantityDates(IEnumerable<APSReportMRPPosition> suggestionPositions, int obtainingPeriod)
        {
			List<QuantityDateModel> quantityDatesAll = new List<QuantityDateModel>();
			QuantityDateModel quantityDateTerminated = GetQuantityDateTerminated(suggestionPositions);
			quantityDatesAll.Add(quantityDateTerminated);

			QuantityDateModel[] quantityDatesForDates = GetQuantityDatesForDates(suggestionPositions, obtainingPeriod);
			quantityDatesAll.AddRange(quantityDatesForDates);
			return quantityDatesAll.ToArray();
		}

		private QuantityDateModel GetQuantityDateTerminated(IEnumerable<APSReportMRPPosition> suggestionPositions)
        {
			Date firstDate = _dates.FirstOrDefault();
            APSReportMRPPosition[] terminatedSuggestionPositions = suggestionPositions
                .Where(x => x.StartDate < firstDate)
                .ToArray();

			Quantity sumQuantity = Quantity.Empty;
			foreach(APSReportMRPPosition suggestionPosition in terminatedSuggestionPositions)
			{
				sumQuantity += suggestionPosition.Quantity;
			}

            Date tomorrowDate = Date.Today.AddDays(-1);
			return new QuantityDateModel
            {
                Quantity = sumQuantity,
                StartDate = tomorrowDate,
                AvailabilityDate = tomorrowDate,
				SuggestionPositions = terminatedSuggestionPositions
			};
		}

        private QuantityDateModel[] GetQuantityDatesForDates(IEnumerable<APSReportMRPPosition> suggestionPositions, int obtainingPeriod)
        {
			List<QuantityDateModel> quantityDates = new List<QuantityDateModel>();
			for(int i = 0; i < _dates.Length - 1; i++)
			{
				Date startDate = _dates[i];
				Date nextStartDate = _dates[i + 1];
				APSReportMRPPosition[] suggestionPositionsDate = suggestionPositions
				   .Where(x => x.StartDate >= startDate && x.StartDate < nextStartDate)
				   .ToArray();

				Quantity sumQuantity = Quantity.Empty;
				if(_configRapMRPOgolneWorker.ReduceSalesPlansByOrderQuantity)
                {
					Quantity sumPZOQuantity = Quantity.Empty;
					Quantity sumZOQuantity = Quantity.Empty;

					foreach(APSReportMRPPosition suggestionPosition in suggestionPositionsDate)
					{
						if(suggestionPosition.RelatedSuggestionParent?.DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.PZO)
						{
							sumPZOQuantity += suggestionPosition.Quantity;
							continue;
						}

						sumZOQuantity += suggestionPosition.Quantity;
					}

                    Quantity diffZZOAndPZO = Quantity.Abs(sumZOQuantity - sumPZOQuantity);
                    if(!diffZZOAndPZO.IsZero)
                    {
                        sumQuantity += diffZZOAndPZO;
					}
                }
                else
                {
					foreach(APSReportMRPPosition suggestionPosition in suggestionPositionsDate)
					{
						sumQuantity += suggestionPosition.Quantity;
					}
				}

				QuantityDateModel quantityDateModel = new QuantityDateModel
                {
                    Quantity = sumQuantity,
                    StartDate = startDate,
                    AvailabilityDate = startDate.AddDays(obtainingPeriod),
                    SuggestionPositions = suggestionPositionsDate
                };
				quantityDates.Add(quantityDateModel);
			}
            return quantityDates.ToArray();
		}

		private List<APSReportMRPElement> GetReportMRPElements()
        {
            RowCondition rc = GetRowConditionReportMRPElements();
            return _session.GetDPSMRPReport().APSRepMRPElems.PrimaryKey[rc]
                .Cast<APSReportMRPElement>()
                .ToList();
        }

        private RowCondition GetRowConditionReportMRPElements()
        {
            RowCondition rc = RowCondition.Empty;
            if (_parametry.ReportMRPType == ReportMRPUIParams.ReportMRPTypeEnum.AllSuggestions)
            {
				return new RowCondition.Exists(
					nameof(APSReportMRP),
					nameof(APSReportMRPPosition.ReportMRPElement),
					new FieldCondition.In(nameof(APSReportMRPPosition.DefinitionCode), [
					    APSReportMRPPositionConfiguration.DefinitionCode.SP,
						APSReportMRPPositionConfiguration.DefinitionCode.SZ
					])
				);
			}

            if (_parametry.ReportMRPType == ReportMRPUIParams.ReportMRPTypeEnum.ProductionSuggestions)
            {
                return new RowCondition.Exists(
                    nameof(APSReportMRP),
					nameof(APSReportMRPPosition.ReportMRPElement),
					new FieldCondition.Equal(nameof(APSReportMRPPosition.DefinitionCode), APSReportMRPPositionConfiguration.DefinitionCode.SP)
				);
            }

            if (_parametry.ReportMRPType == ReportMRPUIParams.ReportMRPTypeEnum.PurchasingSuggestions)
            {
				return new RowCondition.Exists(
					nameof(APSReportMRP),
					nameof(APSReportMRPPosition.ReportMRPElement),
					new FieldCondition.Equal(nameof(APSReportMRPPosition.DefinitionCode), APSReportMRPPositionConfiguration.DefinitionCode.SZ)
				);
			}
            return RowCondition.Empty;
        }

        private List<APSReportMRPPosition> GetReportMRPElementPositionsSorted(APSReportMRPElement reportMRPElement)
        {
            View view = reportMRPElement.Positions.CreateView();
            view.ForceSqlQuery = true;
            view.Sort = nameof(APSReportMRPPosition.AvailabilityDate);

            return view
                .Cast<APSReportMRPPosition>()
                .ToList();
        }

        private List<APSReportMRPPosition> GetReportMRPSuggestionPositions(IEnumerable<APSReportMRPPosition> pozycje)
        {
            switch (_parametry.ReportMRPType)
            {
                case ReportMRPUIParams.ReportMRPTypeEnum.PurchasingSuggestions:
                    return GetRapMRPPozycjeSugestieZakupu(pozycje);
                case ReportMRPUIParams.ReportMRPTypeEnum.ProductionSuggestions:
                    return GetRapMRPPozycjeSugestieProdukcji(pozycje);
            }
            return GetRapMRPPozycjeSugestieWszystkie(pozycje);
        }

        private List<APSReportMRPPosition> GetRapMRPPozycjeSugestieZakupu(IEnumerable<APSReportMRPPosition> pozycje)
        {
            return pozycje
                .Where(x => x.DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.SZ)
                .ToList();
        }

        private List<APSReportMRPPosition> GetRapMRPPozycjeSugestieProdukcji(IEnumerable<APSReportMRPPosition> pozycje)
        {
            return pozycje
                .Where(x => x.DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.SP)
                .ToList();
        }

        private List<APSReportMRPPosition> GetRapMRPPozycjeSugestieWszystkie(IEnumerable<APSReportMRPPosition> pozycje)
        {
            return pozycje
                .Where(x => x.IsSuggestion())
                .ToList();
        }
    }
}
