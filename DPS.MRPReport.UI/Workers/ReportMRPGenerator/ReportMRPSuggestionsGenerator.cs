using Soneta.Business;
using Soneta.Magazyny;
using Soneta.ProdukcjaPro;
using Soneta.Towary;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using DPS.MRPReport.UI.Workers.ReportMRPGenerator.Models;
using DPS.MRPReport.Workers.Config;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Enums;
using DPS.MRPReport.Extensions;
using DPS.MRPReport.Configurations;
using DPS.MRPReport.Utils;
using DPS.MRPReport.UI.Workers.ReportMRPGenerator.Helpers;

namespace DPS.MRPReport.UI.Workers.ReportMRPGenerator
{
	public class ReportMRPSuggestionsGenerator
    {
        private readonly Session _session;
        private readonly Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> _modelsPerTowary = new Dictionary<APSTowarExt, List<APSReportMRPPositionModel>>();
       
		private readonly ConfigReportMRPMainWorker _configRapMRPOgolneWorker;
        private readonly ProTechnologiaUtil _proTechnologiaUtil;
        private readonly APSReportMRPPositionUtil _apsReportMRPPositionUtil;
		private readonly bool _includeOrderingPolicy;

		public ReportMRPSuggestionsGenerator(Session session, Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> rapMRPModelsPerRapMRPTowary)
        {
            _session = session;
            _modelsPerTowary = rapMRPModelsPerRapMRPTowary;
            _configRapMRPOgolneWorker = new ConfigReportMRPMainWorker(session);
            _proTechnologiaUtil = new ProTechnologiaUtil(session);
			_apsReportMRPPositionUtil = new APSReportMRPPositionUtil(session);
			_includeOrderingPolicy = _configRapMRPOgolneWorker.IncludeOrderingPolicy;
		}

        public Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> Generate()
        {
            CalculateStartBalanceQuantityForAll();
            GenerateSuggestions();
            RemoveBOMinModels();
			return _modelsPerTowary;
        }

		private void CalculateStartBalanceQuantity(IEnumerable<APSTowarExt> towaryExts)
		{
			foreach(APSTowarExt towarExt in towaryExts)
			{
				CalculateStartBalanceQuantity(towarExt);
			}
		}

		private void CalculateStartBalanceQuantityForAll()
		{
			foreach(APSTowarExt towarExt in _modelsPerTowary.Keys)
			{
				CalculateStartBalanceQuantity(towarExt);
			}
		}

		private void CalculateStartBalanceQuantity(APSTowarExt towarExt)
		{
			_modelsPerTowary[towarExt] = new APSReportMRPPositionModelHelper().SortModels(_modelsPerTowary[towarExt]);
			APSReportMRPPositionModel positionBOModel = _modelsPerTowary[towarExt].FirstOrDefault();

			Quantity stanMagazynowy = positionBOModel.Quantity;
			for(int i = 1; i < _modelsPerTowary[towarExt].Count; i++)
			{
				APSReportMRPPositionModel positionModel = _modelsPerTowary[towarExt][i];
				if(positionModel.Direction == KierunekPartii.Przychód)
				{
					stanMagazynowy += positionModel.Quantity;
				}
				else
				{
					stanMagazynowy -= positionModel.Quantity;
				}

				positionModel.BalanceQuantity = stanMagazynowy;

				if(_includeOrderingPolicy
					&& (positionModel.TowarExt.IsOrderingPolicyMinimumReserve() || positionModel.TowarExt.IsOrderingPolicyCombination()))
				{
					positionModel.BalanceQuantityCalculations = stanMagazynowy - towarExt.GetMinimalStock(positionModel.AvailabilityDate);
				}
				else
				{
					positionModel.BalanceQuantityCalculations = stanMagazynowy;
				}
			}
		}

		private void GenerateSuggestions()
		{
			foreach(APSTowarExt towarExt in _modelsPerTowary.Keys)
			{
				GenerateSuggestions(towarExt);
			}
		}

		private void GenerateSuggestions(APSTowarExt towarExt)
		{
			for(int i = 0; i < _modelsPerTowary[towarExt].Count; i++)
			{
				APSReportMRPPositionModel reportMRPPositionModel = _modelsPerTowary[towarExt][i];
				if(reportMRPPositionModel.BalanceQuantityCalculations.IsPlusOrZero)
				{
					continue;
				}

				GenerateSuggestion(reportMRPPositionModel);

				i = _modelsPerTowary[towarExt].IndexOf(reportMRPPositionModel);
			}
		}

		private void GenerateSuggestion(APSReportMRPPositionModel positionModel)
        {
			Quantity stanMagazynowy = positionModel.BalanceQuantityCalculations;
			if(stanMagazynowy.IsPlusOrZero)
			{
				return;
			}

			Quantity stanMagazynowyAbs = Quantity.Abs(stanMagazynowy);
			if(positionModel.TowarExt.MRPObtainingMethod == ObtainingMethodEnum.Purchased)
			{
				CreateModelPurchaseSuggestion(positionModel, stanMagazynowyAbs);
				return;
			}
			CreateModelProductionSuggestion(positionModel, stanMagazynowyAbs);
		}

		private void CreateModelPurchaseSuggestion(APSReportMRPPositionModel reportMRPPositionModel, Quantity quantity)
		{
			Date availabilityDate = _apsReportMRPPositionUtil.GetDateTakingObtainingPeriod(reportMRPPositionModel.TowarExt, reportMRPPositionModel.AvailabilityDate);
			APSReportMRPPositionModel purchaseSuggestion = new APSReportMRPPositionModel
			{
				TowarExt = reportMRPPositionModel.TowarExt,
				Quantity = CalculateQuantityAccordingToThePolicy(reportMRPPositionModel.TowarExt, quantity),
				Direction = KierunekPartii.Przychód,
				ObtainingMethod = ObtainingMethodEnum.Purchased,
				DocumentNumber = string.Empty,
				ObtainingPeriod = reportMRPPositionModel.TowarExt.MRPObtainingPeriod,
				DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.SZ,
				StartDate = _apsReportMRPPositionUtil.GetDateTakingObtainingPeriod(reportMRPPositionModel.TowarExt, availabilityDate),
				AvailabilityDate = availabilityDate
			};

			_modelsPerTowary[reportMRPPositionModel.TowarExt].Add(purchaseSuggestion);
			_modelsPerTowary[reportMRPPositionModel.TowarExt] = RecalculateIloscBilans(purchaseSuggestion);

            reportMRPPositionModel.RelatedSuggestion = purchaseSuggestion;
		}

		private void CreateModelProductionSuggestion(APSReportMRPPositionModel positionModel, Quantity quantity)
		{
			ProTechnologia technologia = _proTechnologiaUtil.GetDefaultTechnologia(positionModel.TowarExt.Towar);
			if(technologia is null)
			{
				return;
			}

			Quantity quantitySP = positionModel.TowarExt.Towar.PrzeliczJednostkę(positionModel.TowarExt.Towar.Jednostka, quantity, true);
			APSReportMRPPositionModel positionModelSP = new APSReportMRPPositionModel
			{
				TowarExt = positionModel.TowarExt,
				Quantity = CalculateQuantityAccordingToThePolicy(positionModel.TowarExt, quantity),
				Direction = KierunekPartii.Przychód,
				ObtainingMethod = ObtainingMethodEnum.Manufactured,
				DocumentNumber = string.Empty,
				ObtainingPeriod = positionModel.TowarExt.MRPObtainingPeriod,
				DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.SP,
				StartDate = _apsReportMRPPositionUtil.GetDateTakingObtainingPeriod(positionModel.TowarExt, positionModel.StartDate),
				AvailabilityDate = positionModel.StartDate
			};

			positionModel.RelatedSuggestion = positionModelSP;

			_modelsPerTowary[positionModel.TowarExt].Add(positionModelSP);
			_modelsPerTowary[positionModel.TowarExt] = RecalculateIloscBilans(positionModelSP);

			ProMaterialOperacjiTechnologii[] materials = GetMaterials(positionModel, technologia);
			foreach(ProMaterialOperacjiTechnologii material in materials)
			{
				APSTowarExt apsTowarExtMaterial = material.Towar.GetAPSExt();
				if(!_modelsPerTowary.ContainsKey(apsTowarExtMaterial))
				{
					continue;
				}

				double materialIloscValue = 0;
				if(technologia.IloscPrzeliczona.Value != 0)
				{
					materialIloscValue = material.IloscPrzeliczona.Value / technologia.IloscPrzeliczona.Value;
				}

				Quantity quantityROBase = new Quantity(positionModelSP.Quantity.Value * materialIloscValue, material.IloscPrzeliczona.Symbol);
				Quantity quantityRO = material.Towar.PrzeliczJednostkę(material.Towar.Jednostka, quantityROBase, true);

				APSReportMRPPositionModel reportMRPPositionROModel = new APSReportMRPPositionModel
				{
					Parent = positionModelSP,
					TowarExt = apsTowarExtMaterial,
					Quantity = CalculateQuantityAccordingToThePolicy(apsTowarExtMaterial, quantityRO),
					Direction = KierunekPartii.Rozchód,
					ObtainingMethod = apsTowarExtMaterial.MRPObtainingMethod,
					DocumentNumber = string.Empty,
					ObtainingPeriod = apsTowarExtMaterial.MRPObtainingPeriod,
					DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.RO,
					StartDate = _apsReportMRPPositionUtil.GetDateTakingObtainingPeriod(apsTowarExtMaterial, positionModelSP.StartDate),
					AvailabilityDate = positionModelSP.StartDate
				};

				_modelsPerTowary[apsTowarExtMaterial].Add(reportMRPPositionROModel);
				RecalculateRelatedObjects(reportMRPPositionROModel);
			}
		}

		private ProMaterialOperacjiTechnologii[] GetMaterials(APSReportMRPPositionModel positionModel, ProTechnologia technologia)
		{
			return technologia.Operacje
				.SelectMany(x => x.Materialy)
				.Where(x => x.Towar.Guid != positionModel.TowarExt.Towar.Guid)
				.ToArray();
		}

		private void RecalculateRelatedObjects(APSReportMRPPositionModel positionModel)
		{
			List<APSTowarExt> towaryExtsToRecalculate = [positionModel.TowarExt];
			int index = 0;
			while(index < _modelsPerTowary[positionModel.TowarExt].Count)
			{
				APSReportMRPPositionModel model = _modelsPerTowary[positionModel.TowarExt][index];
				if(model.IsSuggestion())
				{
					APSTowarExt[] towaryExts = RemoveRelatedObjects(model);
					towaryExtsToRecalculate.AddRange(towaryExts);
				}

				index++;
			}

			APSTowarExt[] towaryExtsToRecalculateDist = towaryExtsToRecalculate
				.Distinct()
				.ToArray();

			CalculateStartBalanceQuantity(towaryExtsToRecalculateDist);
			GenerateSuggestions(positionModel.TowarExt);
		}

		private APSTowarExt[] RemoveRelatedObjects(APSReportMRPPositionModel positionModel)
		{
			List<APSTowarExt> towaryExts = [];
			RemoveRelatedObjectsRecursive(positionModel, towaryExts);
			return towaryExts.ToArray();
		}

		private void RemoveRelatedObjectsRecursive(APSReportMRPPositionModel positionModel, List<APSTowarExt> towaryExtsToUpdate)
		{
			_modelsPerTowary[positionModel.TowarExt].Remove(positionModel);
			towaryExtsToUpdate.Add(positionModel.TowarExt);

			APSReportMRPPositionModel relatedModel = _modelsPerTowary[positionModel.TowarExt]
				.FirstOrDefault(x => x.RelatedSuggestion == positionModel);

			if(relatedModel is not null)
			{
				relatedModel.RelatedSuggestion = null;
			}

			if(positionModel.IsProductionSuggestion())
			{
				APSReportMRPPositionModel[] children = _modelsPerTowary
					.SelectMany(x => x.Value)
					.Where(x => x.Parent == positionModel)
					.ToArray();

				if(children.Any())
				{
					// Tutaj będą pozycje RO
					foreach(APSReportMRPPositionModel child in children)
					{
						// Usuwamy RO
						_modelsPerTowary[child.TowarExt].Remove(child);
						if(!towaryExtsToUpdate.Contains(child.TowarExt))
						{
							towaryExtsToUpdate.Add(child.TowarExt);
						}

						if(child.RelatedSuggestion is null)
						{
							continue;
						}

						// Usuwamy powiązaną z RO sugestię, o ile taka była
						_modelsPerTowary[child.TowarExt].Remove(child.RelatedSuggestion);
						RemoveRelatedObjectsRecursive(child.RelatedSuggestion, towaryExtsToUpdate);
					}
				}
			}

			int index = 0;
			while(index < _modelsPerTowary[positionModel.TowarExt].Count)
			{
				APSReportMRPPositionModel model = _modelsPerTowary[positionModel.TowarExt][index];
				if(model.IsSuggestion())
				{
					RemoveRelatedObjects(model);
					continue;
				}

				index++;
			}
		}

		private List<APSReportMRPPositionModel> RecalculateIloscBilans(APSReportMRPPositionModel mrpPositionModelNew)
        {
            List<APSReportMRPPositionModel> mrpRepPozycjaSortedModels = _modelsPerTowary[mrpPositionModelNew.TowarExt]
                .OrderBy(x => x.AvailabilityDate)
                .ThenByDescending(x => (int)x.Direction)
				.ThenBy(x => x.DefinitionCode == APSReportMRPPositionConfiguration.DefinitionCode.BO ? 1 : 0)
				.ToList();

            int indexFrom = mrpRepPozycjaSortedModels.IndexOf(mrpPositionModelNew) - 1;
           
            Quantity stanMagazynowy = mrpRepPozycjaSortedModels[indexFrom].BalanceQuantity;
            for (int i = indexFrom + 1; i < mrpRepPozycjaSortedModels.Count; i++)
            {
                APSReportMRPPositionModel pozycjaModel = mrpRepPozycjaSortedModels[i];
                if (pozycjaModel.Direction == KierunekPartii.Przychód)
                {
                    stanMagazynowy += pozycjaModel.Quantity;
                }
                else
                {
                    stanMagazynowy -= pozycjaModel.Quantity;
                }

                pozycjaModel.BalanceQuantity = stanMagazynowy;

                if (_includeOrderingPolicy
					&& (pozycjaModel.TowarExt.IsOrderingPolicyMinimumReserve() || pozycjaModel.TowarExt.IsOrderingPolicyCombination()))
                {
                    pozycjaModel.BalanceQuantityCalculations = stanMagazynowy - pozycjaModel.TowarExt.GetMinimalStock(pozycjaModel.AvailabilityDate);
                }
                else
                {
					pozycjaModel.BalanceQuantityCalculations = stanMagazynowy;
                }
            }
            return mrpRepPozycjaSortedModels;
        }

		private Quantity CalculateQuantityAccordingToThePolicy(APSTowarExt apsTowar, Quantity quantity)
		{
			if(!_includeOrderingPolicy)
			{
				return quantity;
			}

			if(quantity < apsTowar.MRPLogisticalMinimum)
			{
				quantity = apsTowar.MRPLogisticalMinimum;
			}

			if(apsTowar.IsOrderingPolicyFixedQuantity()
				|| apsTowar.IsOrderingPolicyCombination())
			{
                double factor = 1;
                if(!apsTowar.MRPOrderingQuantity.IsZero)
                {
					factor = Math.Ceiling(quantity.Value / apsTowar.MRPOrderingQuantity.Value);
				}

				return new Quantity(factor * apsTowar.MRPOrderingQuantity.Value, quantity.Symbol);
			}
			return quantity;
		}

        private void RemoveBOMinModels()
        {
            foreach(var reportMRPModelsPerAPSTowar in _modelsPerTowary)
            {
                reportMRPModelsPerAPSTowar.Value.RemoveAt(0);
			}
        }
	}
}
