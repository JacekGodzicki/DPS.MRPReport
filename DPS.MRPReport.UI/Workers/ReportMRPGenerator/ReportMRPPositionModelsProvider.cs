using Soneta.Business;
using Soneta.Handel;
using Soneta.Magazyny;
using Soneta.ProdukcjaPro;
using Soneta.Towary;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using DPS.MRPReport.UI.Workers.ReportMRPGenerator.Models;
using DPS.MRPReport.Workers.Config;
using DPS.MRPReport.Tables.Extensions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Configurations;
using DPS.MRPReport.Utils;
using DPS.MRPReport.UI.Workers.ReportMRPGenerator.Helpers;
using DPS.MRPReport.Extensions;

namespace DPS.MRPReport.UI.Workers.ReportMRPGenerator
{
	public class ReportMRPPositionModelsProvider
    {
        public class Params
        {
            public required Date DateFrom { get; set; }
            public required Date DateTo { get; set; }
            public required Magazyn[] Magazyny { get; set; }
            public required APSTowarExt[] RapMRPTowary { get; set; }
            public required Towar[] Towary { get; set; }
        }

        private readonly Session _session;
        private readonly Date _dateFrom;
        private readonly Date _dateTo;
        private readonly Magazyn[] _magazyny;
		private readonly APSTowarExt[] _apsTowary;
        private readonly Towar[] _towary;
        private readonly ConfigReportMRPMainWorker _configReportMRPMainWorker;
        private readonly APSReportMRPPositionUtil _apsReportMRPPositionUtil;

        private Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> _rapMRPModelsPerRapMRPTowary = new Dictionary<APSTowarExt, List<APSReportMRPPositionModel>>();

        public ReportMRPPositionModelsProvider(Session session, Params pars)
        {
            _session = session;
			_dateFrom = pars.DateFrom;
            _dateTo = pars.DateTo;
            _magazyny = pars.Magazyny;
            _apsTowary = pars.RapMRPTowary;
            _towary = pars.Towary;
            _configReportMRPMainWorker = new ConfigReportMRPMainWorker(_session);
            _apsReportMRPPositionUtil = new APSReportMRPPositionUtil(_session);
		}

        public Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> GetModelsPerTowary()
        {
            List<APSReportMRPPositionModel> modelsBO = GetModelsBO();
            List<APSReportMRPPositionModel> modelsDokHandlowe = GetModelsDokHandlowe();
			List<APSReportMRPPositionModel> modelsZAP = GetModelsZAP();
			List<APSReportMRPPositionModel> modelsZP = GetModelsZP();

			List<APSReportMRPPositionModel> models = [.. modelsBO, .. modelsDokHandlowe, .. modelsZAP, .. modelsZP];
            return new APSReportMRPPositionModelHelper().GroupAndSortModels(models);
        }

		private List<APSReportMRPPositionModel> GetModelsBO()
        {
            StanMagazynuWorker stanMagazynuWorker = new StanMagazynuWorker();
            stanMagazynuWorker.Magazyny = _magazyny;
            stanMagazynuWorker.Data = _dateFrom;

            List<APSReportMRPPositionModel> models = new List<APSReportMRPPositionModel>();
            foreach (APSTowarExt apsTowar in _apsTowary)
            {
                Quantity stanRazem = Quantity.Empty;
                if (_magazyny.Any())
                {
                    stanMagazynuWorker.Towar = apsTowar.Towar;
                    stanRazem = stanMagazynuWorker.StanRazem;

                    if(stanRazem == Quantity.Empty)
                    {
                        stanRazem = new Quantity(0, apsTowar.Towar.Jednostka.Kod);
					}
                }

                APSReportMRPPositionModel modelBOMin = CreateReportMRPPositionBOModelMin(apsTowar);
				models.Add(modelBOMin);

				APSReportMRPPositionModel modelBO = CreateReportMRPPositionBOModel(apsTowar, stanRazem);
                models.Add(modelBO);
            }
            return models;
        }

        private APSReportMRPPositionModel CreateReportMRPPositionBOModel(APSTowarExt apsTowar, Quantity ilosc)
        {
            return new APSReportMRPPositionModel
            {
                TowarExt = apsTowar,
                Quantity = ilosc,
                Direction = KierunekPartii.Przychód,
                DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.BO,
                DocumentNumber = string.Empty,
                ObtainingMethod = apsTowar.MRPObtainingMethod,
                ObtainingPeriod = apsTowar.MRPObtainingPeriod,
                StartDate = _dateFrom,
			    AvailabilityDate = _dateFrom,
                BalanceQuantity = ilosc,
                BalanceQuantityCalculations = ilosc
            };
        }

		private APSReportMRPPositionModel CreateReportMRPPositionBOModelMin(APSTowarExt apsTowar)
        {
            Quantity quantityZero = new Quantity(0, apsTowar.Towar.Jednostka.Kod);
			return new APSReportMRPPositionModel
			{
				TowarExt = apsTowar,
				Quantity = quantityZero,
				Direction = KierunekPartii.Przychód,
				DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.BO,
				DocumentNumber = string.Empty,
				ObtainingMethod = apsTowar.MRPObtainingMethod,
				ObtainingPeriod = apsTowar.MRPObtainingPeriod,
                StartDate = Date.MinValue,
				AvailabilityDate = Date.MinValue,
				BalanceQuantity = quantityZero,
				BalanceQuantityCalculations = quantityZero
			};
		}

		private List<APSReportMRPPositionModel> GetModelsDokHandlowe()
        {
            List<APSReportMRPPositionModel> models = new List<APSReportMRPPositionModel>();
            APSPozycjaDokHandlowegoExt[] pozycjaExts = GetAPSPozycjaDokHandlowegoExts();
            foreach (APSPozycjaDokHandlowegoExt pozycjaExt in pozycjaExts)
            {
                APSReportMRPPositionModel positionModel = CreateReportMRPPositionModel(pozycjaExt);
                models.Add(positionModel);
            }
            return models;
        }

		private List<APSReportMRPPositionModel> GetModelsZAP()
		{
			if(!_configReportMRPMainWorker.IncludeRequisitionDocuments
                || _configReportMRPMainWorker.RequisitionDocumentDefs is null
                || !_configReportMRPMainWorker.RequisitionDocumentDefs.Any())
			{
				return [];
			}

            APSPozycjaDokHandlowegoExt[] pozycjeExtZAP = GetPozycjeExtZAP();

			List<APSReportMRPPositionModel> models = new List<APSReportMRPPositionModel>();
			foreach(APSPozycjaDokHandlowegoExt pozycjaExtZAP in pozycjeExtZAP)
            {
				APSReportMRPPositionModel model = CreateReportMRPPositionModel(pozycjaExtZAP);
				models.Add(model);
			}
            return models;
		}

        private APSPozycjaDokHandlowegoExt[] GetPozycjeExtZAP()
        {
			RowCondition rc = new FieldCondition.In(PropertyPathUtil.GetPropertyPath<APSPozycjaDokHandlowegoExt>(x => x.PozycjaDokHandlowego.Dokument.Definicja), _configReportMRPMainWorker.RequisitionDocumentDefs);
			if(!_configReportMRPMainWorker.IncludeDocsInBuffer)
			{
				rc &= new FieldCondition.In(PropertyPathUtil.GetPropertyPath<APSPozycjaDokHandlowegoExt>(x => x.PozycjaDokHandlowego.Dokument.Stan), [StanDokumentuHandlowego.Zatwierdzony, StanDokumentuHandlowego.Zablokowany]);
			}
			else
			{
				rc &= new FieldCondition.In(PropertyPathUtil.GetPropertyPath<APSPozycjaDokHandlowegoExt>(x => x.PozycjaDokHandlowego.Dokument.Stan), [StanDokumentuHandlowego.Zatwierdzony, StanDokumentuHandlowego.Zablokowany, StanDokumentuHandlowego.Bufor]);
			}

			return _session.GetDPSMRPReport().APSPozDHExt.PrimaryKey[rc]
			    .Cast<APSPozycjaDokHandlowegoExt>()
			    .ToArray();
		}

		private APSReportMRPPositionModel CreateReportMRPPositionModel(APSPozycjaDokHandlowegoExt pozycjaExt)
		{
			APSTowarExt apsTowar = _apsTowary.FirstOrDefault(x => x.Towar == pozycjaExt.PozycjaDokHandlowego.Towar);
			string definitionCode = GetDefinitionCodeForDokumentHandlowy(pozycjaExt.PozycjaDokHandlowego.Dokument);

			return new APSReportMRPPositionModel
			{
				TowarExt = apsTowar,
				Quantity = pozycjaExt.PozycjaDokHandlowego.IloscMagazynu,
				Direction = pozycjaExt.PozycjaDokHandlowego.KierunekMagazynu,
				DefinitionCode = definitionCode,
				DocumentNumber = pozycjaExt.PozycjaDokHandlowego.Dokument.Numer.NumerPelny,
				ObtainingMethod = apsTowar.MRPObtainingMethod,
				ObtainingPeriod = apsTowar.MRPObtainingPeriod,
				StartDate = _apsReportMRPPositionUtil.GetDateTakingObtainingPeriod(apsTowar, pozycjaExt.DeliveryDate),
				AvailabilityDate = pozycjaExt.DeliveryDate
			};
		}

        private string GetDefinitionCodeForDokumentHandlowy(DokumentHandlowy dokumentHandlowy)
        {
            if(dokumentHandlowy.Kategoria == KategoriaHandlowa.ZamówienieDostawcy)
            {
                return APSReportMRPPositionConfiguration.DefinitionCode.ZD;
            }

            if(dokumentHandlowy.Kategoria == KategoriaHandlowa.ZamówienieOdbiorcy)
            {
				return APSReportMRPPositionConfiguration.DefinitionCode.ZO;
			}
            return APSReportMRPPositionConfiguration.DefinitionCode.ZAP;
		}

		private List<APSReportMRPPositionModel> GetModelsZP()
        {
            List<APSReportMRPPositionModel> models = new List<APSReportMRPPositionModel>();

            ProZlecenie[] zleceniaProdukcyjne = GetProZlecenia();
            foreach (ProZlecenie proZlecenie in zleceniaProdukcyjne)
            {
                APSTowarExt rapMRPTowar = _apsTowary.FirstOrDefault(x => x.Towar == proZlecenie.Towar);
                APSReportMRPPositionModel mrpReportPozycjaZPModel = CreateRapMRPPozycjaZPModel(proZlecenie, rapMRPTowar);
                models.Add(mrpReportPozycjaZPModel);
                
                IEnumerable<ProMaterialOperacjiZlecenia> materialy = GetMaterialyForProZlecenie(proZlecenie);
                foreach (ProMaterialOperacjiZlecenia material in materialy)
                {
                    APSTowarExt apsTowar = _apsTowary.FirstOrDefault(x => x.Towar.Guid == material.Towar.Guid);
                    if(apsTowar is null)
                    {
                        continue;
                    }

                    APSReportMRPPositionModel mrpRepPozycjaMaterialModel = CreateRapMRPPozycjaMaterialModel(material, apsTowar, mrpReportPozycjaZPModel);
                    models.Add(mrpRepPozycjaMaterialModel);
                }
            }
            return models;
        }

		private APSReportMRPPositionModel CreateRapMRPPozycjaZPModel(ProZlecenie proZlecenie, APSTowarExt apsTowar)
        {
			return new APSReportMRPPositionModel
            {
                TowarExt = apsTowar,
                Quantity = proZlecenie.IloscPrzeliczona,
                Direction = KierunekPartii.Przychód,
                DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.ZP,
                DocumentNumber = proZlecenie.Numer.NumerPelny,
                ObtainingMethod = apsTowar.MRPObtainingMethod,
                ObtainingPeriod = apsTowar.MRPObtainingPeriod,
                StartDate = _apsReportMRPPositionUtil.GetDateTakingObtainingPeriod(apsTowar, proZlecenie.Zakonczenie),
                AvailabilityDate = proZlecenie.Zakonczenie
            };
        }

        private IEnumerable<ProMaterialOperacjiZlecenia> GetMaterialyForProZlecenie(ProZlecenie proZlecenie)
        {
            // Wykluczamy towary będące tym samym co towar produkowany z technologii
            return proZlecenie.Operacje
                .SelectMany(x => x.Materialy)
                .Where(x => x.Towar.Guid != proZlecenie.Towar.Guid)
                .ToList();
        }

        private APSReportMRPPositionModel CreateRapMRPPozycjaMaterialModel(ProMaterialOperacjiZlecenia material,
            APSTowarExt apsTowar, APSReportMRPPositionModel parentModel)
        {
            return new APSReportMRPPositionModel
            {
                Parent = parentModel,
				TowarExt = apsTowar,
                Quantity = material.IloscPrzeliczona,
                Direction = KierunekPartii.Rozchód,
                DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.PR,
                DocumentNumber = material.Operacja.Zlecenie.Numer.NumerPelny,
                ObtainingMethod = apsTowar.MRPObtainingMethod,
                ObtainingPeriod = apsTowar.MRPObtainingPeriod,
                StartDate = _apsReportMRPPositionUtil.GetDateTakingObtainingPeriod(apsTowar, material.Operacja.Zakonczenie),
				AvailabilityDate = material.Operacja.Zakonczenie
            };
        }

		private APSPozycjaDokHandlowegoExt[] GetAPSPozycjaDokHandlowegoExts()
        {
			APSPozycjaDokHandlowegoExt[] ordersToRecipientPositions = GetOrdersToRecipientPositions();
            APSPozycjaDokHandlowegoExt[] ordersToSuppliersPositions = GetOrdersToSuppliersPositions();
			List<APSPozycjaDokHandlowegoExt> positions = [..ordersToRecipientPositions, ..ordersToSuppliersPositions];
			return positions
                .OrderBy(x => x.DeliveryDate)
			    .ToArray();
        }

        private APSPozycjaDokHandlowegoExt[] GetOrdersToRecipientPositions()
        {
			DefDokHandlowego[] ordersToRecipientDefs = _configReportMRPMainWorker.OrdersToRecipientsDefs;
            if(!ordersToRecipientDefs.Any())
            {
                return [];
            }

			if(_configReportMRPMainWorker.IncludeOrdersToRecipientsCorrections)
			{
				DefDokHandlowego[] ordersToRecipientsCorrectionDefs = ordersToRecipientDefs
				    .Select(x => x.RelacjaKorekty)
				    .Where(x => x is not null && !x.Blokada)
				    .Distinct()
				    .ToArray();

				RowCondition rcWithCorrections = GetRowConditionRapMRPPozycjaDokHandlowegoExts([.. ordersToRecipientDefs, .. ordersToRecipientsCorrectionDefs]);
				return _session.GetDPSMRPReport().APSPozDHExt.PrimaryKey[rcWithCorrections]
					.Cast<APSPozycjaDokHandlowegoExt>()
					.Select(x => x.PozycjaDokHandlowego.PozycjaKorygującaOstatnia)
                    .Where(x => !x.IloscMagazynu.IsZero)
                    .Select(x => x.GetAPSExt())
                    .Distinct()
					.ToArray();
			}

			RowCondition rcWithoutCorrections = GetRowConditionRapMRPPozycjaDokHandlowegoExts(ordersToRecipientDefs);
			return _session.GetDPSMRPReport().APSPozDHExt.PrimaryKey[rcWithoutCorrections]
				.Cast<APSPozycjaDokHandlowegoExt>()
				.Where(x => !x.PozycjaDokHandlowego.IloscMagazynu.IsZero)
                .Distinct()
				.ToArray();
		}

        private APSPozycjaDokHandlowegoExt[] GetOrdersToSuppliersPositions()
        {
			DefDokHandlowego[] ordersToSupplierDefs = _configReportMRPMainWorker.OrdersToSupplierDefs;
			if(!ordersToSupplierDefs.Any())
			{
                return [];
			}

			if(_configReportMRPMainWorker.IncludeOrdersToSupplierCorrections)
			{
				DefDokHandlowego[] ordersToSupplierCorrectionDefs = ordersToSupplierDefs
				    .Select(x => x.RelacjaKorekty)
				    .Where(x => x is not null && !x.Blokada)
				    .Distinct()
				    .ToArray();

				RowCondition rcWithCorrections = GetRowConditionRapMRPPozycjaDokHandlowegoExts([.. ordersToSupplierDefs, .. ordersToSupplierCorrectionDefs]);
				return _session.GetDPSMRPReport().APSPozDHExt.PrimaryKey[rcWithCorrections]
					.Cast<APSPozycjaDokHandlowegoExt>()
					.Select(x => x.PozycjaDokHandlowego.PozycjaKorygującaOstatnia)
					.Where(x => !x.IloscMagazynu.IsZero)
					.Select(x => x.GetAPSExt())
                    .Distinct()
					.ToArray();
			}

			RowCondition rcWithoutCorrections = GetRowConditionRapMRPPozycjaDokHandlowegoExts(ordersToSupplierDefs);
			return _session.GetDPSMRPReport().APSPozDHExt.PrimaryKey[rcWithoutCorrections]
			    .Cast<APSPozycjaDokHandlowegoExt>()
				.Where(x => !x.PozycjaDokHandlowego.IloscMagazynu.IsZero)
                .Distinct()
				.ToArray();
		}

		private RowCondition GetRowConditionRapMRPPozycjaDokHandlowegoExts(DefDokHandlowego[] defsDokHandlowego)
        {
			RowCondition rc = new FieldCondition.In($"{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego)}" +
                $".{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego.Towar)}", _towary);

            rc &= new FieldCondition.In($"{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego)}" +
                $".{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego.Dokument)}" +
                $".{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego.Dokument.Magazyn)}", _magazyny);

            rc &= new FieldCondition.In($"{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego)}" +
                $".{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego.Dokument)}" +
                $".{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego.Dokument.Definicja)}", defsDokHandlowego);

            rc &= new FieldCondition.Contain(nameof(APSPozycjaDokHandlowegoExt.DeliveryDate), _dateFrom, _dateTo);

            List<object> stanyDokumentow = [StanDokumentuHandlowego.Zatwierdzony, StanDokumentuHandlowego.Zablokowany];
            if (_configReportMRPMainWorker.IncludeDocsInBuffer)
            {
                stanyDokumentow.Add(StanDokumentuHandlowego.Bufor);
            }

            rc &= new FieldCondition.In($"{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego)}" +
               $".{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego.Dokument)}" +
               $".{nameof(APSPozycjaDokHandlowegoExt.PozycjaDokHandlowego.Dokument.Stan)}", stanyDokumentow.ToArray());

            return rc;
        }

        private ProZlecenie[] GetProZlecenia()
        {
            RowCondition rc = new FieldCondition.In(nameof(ProZlecenie.Towar), _towary);
            rc &= new FieldCondition.GreaterEqual(nameof(ProZlecenie.Zakonczenie), (DateTime)_dateFrom);
            rc &= new FieldCondition.LessEqual(nameof(ProZlecenie.Zakonczenie), (DateTime)_dateTo);

            View view = _session.GetProdukcjaPro().ProZlecenia.PrimaryKey[rc].CreateView();
            view.ForceSqlQuery = true;
            view.Sort = nameof(ProZlecenie.Zakonczenie);

            return _session.GetProdukcjaPro().ProZlecenia.PrimaryKey[rc]
                .Cast<ProZlecenie>()
                .ToArray();
        }

		private Dictionary<APSTowarExt, List<APSReportMRPPositionModel>> GroupAndSortModels(IEnumerable<APSReportMRPPositionModel> models)
		{
			return models
				.GroupBy(x => x.TowarExt)
                .OrderBy(kv => (int)kv.Key.MRPObtainingMethod)
                .ToDictionary(
					kv => kv.Key,
					kv => kv
					    .OrderBy(x => x.AvailabilityDate)
					    .ThenByDescending(x => (int)x.Direction)
					    .ToList()
				);
		}
	}
}
