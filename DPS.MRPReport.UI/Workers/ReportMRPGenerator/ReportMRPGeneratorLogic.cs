using DPS.MRPReport.Configurations;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.UI.Workers.ReportMRPGenerator.Models;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.Magazyny;
using Soneta.Towary;
using Soneta.Types;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.ReportMRPGenerator
{
	public class ReportMRPGeneratorLogic
    {
        private readonly Session _session;
        private readonly Date _dateFrom;
        private readonly Date _dateTo;

        private APSTowarExt[] _reportMRPTowary = [];
        private Towar[] _towary = [];

        public ReportMRPGeneratorLogic(Session session, Date dateFrom)
        {
            _session = session;
			_dateFrom = dateFrom;
            _dateTo = GetDateTo();
		}

		public void Generate()
        {
            ClearReportMRPTable();
            GenerateReportMRPPositions();
        }

        private Date GetDateTo()
        {
			ConfigReportMRPMainWorker configReportMRPMainWorker = new ConfigReportMRPMainWorker(_session);
			if(configReportMRPMainWorker.LimitNumberOfDaysAheadForCalc)
			{
				return _dateFrom.AddDays(configReportMRPMainWorker.NumberOfDaysAheadForCalc);
			}
            return Date.MaxValue;
		}

        private void ClearReportMRPTable()
        {
			using(ITransaction transaction = _session.Logout(true))
			{
				APSReportMRPPositionRel[] rels = _session.GetDPSMRPReport().APSRepMRPPosRel
					.Cast<APSReportMRPPositionRel>()
					.ToArray();

				foreach(APSReportMRPPositionRel rel in rels)
				{
                    if(rel.State == RowState.Deleted)
                    {
                        continue;
                    }

					rel.Delete();
				}

				transaction.Commit();
			}

			using(ITransaction transaction = _session.Logout(true))
			{
				APSReportMRPPosition[] positions = _session.GetDPSMRPReport().APSReportMRP
					.Cast<APSReportMRPPosition>()
					.ToArray();

				foreach(APSReportMRPPosition position in positions)
				{
					if(position.State == RowState.Deleted)
					{
						continue;
					}

					position.Delete();
				}

				transaction.Commit();
			}

			using(ITransaction transaction = _session.Logout(true))
			{
				APSReportMRPElement[] reportMRPElems = _session.GetDPSMRPReport().APSRepMRPElems
					.Cast<APSReportMRPElement>()
					.ToArray();

				foreach(APSReportMRPElement elem in reportMRPElems)
				{
					elem.Delete();
				}

				transaction.Commit();
			}
        }

        private void GenerateReportMRPPositions()
        {
            _reportMRPTowary = GetAPSTowaryExt();
            _towary = GetTowaryFromAPSTowaryExt(_reportMRPTowary);

            Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> modelsPerRapMRPTowary = GetModelsPerAPSTowarExts();
			Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> modelsPerRapMRPTowaryWithSuggestions = GenerateSuggestions(modelsPerRapMRPTowary);
            CreateReportMRPPositions(modelsPerRapMRPTowaryWithSuggestions);
            SetReportMRPDataOstatniePrzeliczenie();
        }

        private void SetReportMRPDataOstatniePrzeliczenie()
        {
            using(Session subSession = _session.Login.CreateSession(false, true))
            {
				using(ITransaction transaction = subSession.Logout(true))
				{
					subSession.Global.Features[FeatureConstants.FeatureNamesGlobal.ReportMRPDataOstatniePrzeliczenie] = _dateFrom;

					transaction.CommitUI();
				}

				subSession.Save();
            }
        }

        private Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> GenerateSuggestions(Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> reportMRPModelsPerAPSTowary)
        {
            return new ReportMRPSuggestionsGenerator(_session, reportMRPModelsPerAPSTowary).Generate();
        }

        private void CreateReportMRPPositions(Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> mrpPositionsPerModels)
        {
            Dictionary<APSReportMRPPositionModel, APSReportMRPPosition> modelsPerPosition = new Dictionary<APSReportMRPPositionModel, APSReportMRPPosition>();

            using(ITransaction transaction = _session.Logout(true))
            {
				foreach(APSTowarExt towarExt in _reportMRPTowary)
                {
                    IEnumerable<APSReportMRPPositionModel> models = mrpPositionsPerModels[towarExt];
					APSReportMRPElement reportMRPElement = _session.GetDPSMRPReport().APSRepMRPElems.WgTowar[towarExt.Towar];
                    if(reportMRPElement is null)
                    {
                        reportMRPElement = new APSReportMRPElement(towarExt.Towar);
                        _session.AddRow(reportMRPElement);
					}

					foreach(APSReportMRPPositionModel model in models)
                    {
						APSReportMRPPosition reportMRPPosition = new APSReportMRPPosition(reportMRPElement);
						_session.AddRow(reportMRPPosition);
						reportMRPPosition.Quantity = model.Quantity;
						reportMRPPosition.Direction = model.Direction;
						reportMRPPosition.DefinitionCode = model.DefinitionCode;
						reportMRPPosition.DocumentNumber = model.DocumentNumber;
						reportMRPPosition.ObtainingPeriod = model.ObtainingPeriod;
						reportMRPPosition.StartDate = model.StartDate;
						reportMRPPosition.AvailabilityDate = model.AvailabilityDate;
						reportMRPPosition.BalanceQuantity = model.BalanceQuantity;

						modelsPerPosition.Add(model, reportMRPPosition);
					}
				}

                transaction.CommitUI();
			}

            IEnumerable<APSReportMRPPositionModel> modelsWithParent = mrpPositionsPerModels
                .SelectMany(x => x.Value)
                .Where(x => x.Parent is not null);

			using(ITransaction transaction = _session.Logout(true))
            {
				foreach(APSReportMRPPositionModel model in modelsWithParent)
				{
                    APSReportMRPPosition position = modelsPerPosition[model];
				    APSReportMRPPosition positionParent = modelsPerPosition[model.Parent];
				    position.SetParent(positionParent);
                }

                transaction.CommitUI();
			}

			IEnumerable<APSReportMRPPositionModel> modelsWithRelatedSuggestions = mrpPositionsPerModels
				.SelectMany(x => x.Value)
				.Where(x => x.RelatedSuggestion is not null);

			using(ITransaction transaction = _session.Logout(true))
			{
				foreach(APSReportMRPPositionModel model in modelsWithRelatedSuggestions)
				{
					APSReportMRPPosition position = modelsPerPosition[model];
					APSReportMRPPosition positionSuggestion = modelsPerPosition[model.RelatedSuggestion];
                    position.SetRelatedSuggestion(positionSuggestion);
				}

				transaction.CommitUI();
			}
		}

        private APSTowarExt[] GetAPSTowaryExt()
        {
            RowCondition rc = new FieldCondition.Equal(nameof(APSTowarExt.MRPIsReportMRP), true);
            return _session.GetDPSMRPReport().APSTowaryExt.PrimaryKey[rc]
			    .Cast<APSTowarExt>()
			    .ToArray();
		}

        private Towar[] GetTowaryFromAPSTowaryExt(IEnumerable<APSTowarExt> apsTowaryExt)
        {
            return apsTowaryExt
                .Select(x => x.Towar)
                .ToArray();
        }

        private Magazyn[] GetMagazyny()
        {
            RowCondition rc = new FieldCondition.Equal(nameof(APSMagazynExt.IsReportMRP), true);
            APSMagazynExt[] rapMRPmagazyny = _session.GetDPSMRPReport().APSMagExt.PrimaryKey[rc]
                .Cast<APSMagazynExt>()
                .ToArray();

            return rapMRPmagazyny
                .Select(x => x.Magazyn)
                .ToArray();
        }

        private Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> GetModelsPerAPSTowarExts()
        {
            ReportMRPPositionModelsProvider.Params modelsProviderParams = new ReportMRPPositionModelsProvider.Params
            {
                DateFrom = _dateFrom,
                DateTo = _dateTo,
                Magazyny = GetMagazyny(),
                RapMRPTowary = _reportMRPTowary,
                Towary = _towary
            };

            return new ReportMRPPositionModelsProvider(_session, modelsProviderParams).GetModelsPerTowary();
        }
    }
}