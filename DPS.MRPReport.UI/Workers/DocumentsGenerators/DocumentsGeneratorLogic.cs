using DPS.MRPReport.Configurations;
using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.Enums;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator.Models;
using DPS.MRPReport.UI.Workers.ReportMRPGenerator;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Handel;
using Soneta.Magazyny;
using Soneta.Towary;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators
{
	public class DocumentsGeneratorLogic
    {
        public class Params
        {
            public required DefDokHandlowego DefDokHandlowego { get; set; }
            public required Magazyn Magazyn { get; set; }
            public Kontrahent Kontrahent { get; set; }
            public required OrderDocumentCreatorModel[] Models { get; set; }
            public GroupingTypeEnum RodzajGrupowania { get; set; }
        }

        protected readonly Session _session;
        protected readonly DefDokHandlowego _defDokHandlowego;
        protected readonly Magazyn _magazyn;
        protected readonly Kontrahent _kontrahent;
        protected readonly OrderDocumentCreatorModel[] _models;
        protected readonly GroupingTypeEnum _rodzajGrupowania;
        protected readonly ConfigReportMRPMainWorker _configRapMRPOgolneWorker;

        protected DokumentHandlowy _dokumentHandlowy;

        public DocumentsGeneratorLogic(Session session, Params pars)
        {
            _session = session;
            _defDokHandlowego = pars.DefDokHandlowego;
            _magazyn = pars.Magazyn;
            _kontrahent = pars.Kontrahent;
            _models = pars.Models;
            _rodzajGrupowania = pars.RodzajGrupowania;
            _configRapMRPOgolneWorker = new ConfigReportMRPMainWorker(session);

            _session.Changed += SessionChanged;
            _session.Disposed += SessionDisposed;
            _session.Saved += SessionSaved;
        }

        public DokumentHandlowy Create()
        {
            _dokumentHandlowy = CreateDokumentHandlowyAndPozycje();
            if (ShouldSetStanZatwierdzony())
            {
                SetStanZatwierdzony(_dokumentHandlowy);
            }
            return _dokumentHandlowy;
        }

        private void SessionChanged(object sender, EventArgs e)
        {
            if (_dokumentHandlowy is null)
            {
                return;
            }

            if (_dokumentHandlowy.IsDeletedOrDetached())
            {
                SetModelsExistsOrderFalse();
                _session.Changed -= SessionChanged;
            }
        }

        private void SessionDisposed(object sender, EventArgs e)
        {
            _session.Login.SetValue(typeof(DateTime), "ReportMRPUIParamsLoadDateTime", DateTime.Now);
        }

        private void SessionSaved(object sender, EventArgs e)
        {
            if (_dokumentHandlowy.IsDeletedOrDetached())
            {
                return;
            }

            using (Session newSession = _session.Login.CreateSession())
            {
                ConfigReportMRPMainWorker configWorker = new ConfigReportMRPMainWorker(newSession);
                if (configWorker.RecalculateAfterCreatingDocumentOrOrder)
                {
                    GenerateUpdatedRaportMRP(newSession);
                }
                else
                {
                    UpdateSuggestionPositions(newSession);
                }

                newSession.Save();
            }
        }

        private void GenerateUpdatedRaportMRP(Session newSession)
        {
            Date dateFrom = newSession.Global.Features.GetDate(FeatureConstants.FeatureNamesGlobal.ReportMRPDataOstatniePrzeliczenie);
            new ReportMRPGeneratorLogic(newSession, dateFrom).Generate();
        }

        private void UpdateSuggestionPositions(Session newSession)
        {
            APSReportMRPPosition[] positionsSession = _models
                .SelectMany(x => x.SuggestionPositions)
                .Select(x => newSession.Get(x))
                .ToArray();

            using (ITransaction transaction = newSession.Logout(true))
            {
                foreach (APSReportMRPPosition position in positionsSession)
                {
					position.DefinitionCode = GetDefinitionCode();
					position.DocumentNumber = _dokumentHandlowy.Numer.NumerPelny;
                    position.CreatedFromSuggestion = true;
                }

                transaction.Commit();
            }
        }

        private string GetDefinitionCode()
        {
			if(_dokumentHandlowy.Kategoria == KategoriaHandlowa.ZamówienieDostawcy)
			{
				return APSReportMRPPositionConfiguration.DefinitionCode.ZD;
			}
			return APSReportMRPPositionConfiguration.DefinitionCode.ZAP;
		}


		private void SetModelsExistsOrderFalse()
        {
            foreach (var model in _models)
            {
                model.ExistsOrder = false;
            }
        }

        private bool ShouldSetStanZatwierdzony()
        {
            return !_configRapMRPOgolneWorker.IncludeDocsInBuffer
                || _configRapMRPOgolneWorker.IncludeDocsInBuffer && _configRapMRPOgolneWorker.ApproveDocuments;
        }

        private void SetStanZatwierdzony(DokumentHandlowy dokumentHandlowy)
        {
            using (ITransaction transaction = _session.Logout(true))
            {
                dokumentHandlowy.Stan = StanDokumentuHandlowego.Zatwierdzony;

                transaction.CommitUI();
            }
        }

        private DokumentHandlowy CreateDokumentHandlowyAndPozycje()
        {
            DokumentHandlowy dokumentHandlowy;
            using (ITransaction transaction = _session.Logout(true))
            {
                dokumentHandlowy = CreateDokumentHandlowy();
                CreatePositions(dokumentHandlowy);

                transaction.CommitUI();
            }
            return dokumentHandlowy;
        }

        private DokumentHandlowy CreateDokumentHandlowy()
        {
            DokumentHandlowy dokumentHandlowy = new DokumentHandlowy();
            dokumentHandlowy.Definicja = _defDokHandlowego;
            dokumentHandlowy.Magazyn = _magazyn;
            dokumentHandlowy.Kontrahent = _kontrahent;
            _session.AddRow(dokumentHandlowy);
            return dokumentHandlowy;
        }

        private void CreatePositions(DokumentHandlowy dokumentHandlowy)
        {
            switch (_rodzajGrupowania)
            {
                case GroupingTypeEnum.Group:
                    CreatePositionsGroup(dokumentHandlowy);
                    break;
                case GroupingTypeEnum.Separately:
                default:
                    CreatePositionsSeparately(dokumentHandlowy);
                    break;
            }
        }

        private void CreatePositionsGroup(DokumentHandlowy dokumentHandlowy)
        {
            Dictionary<Towar, OrderDocumentCreatorModel[]> modelsPerTowary = _models
                .GroupBy(x => x.Towar)
                .ToDictionary(kv => kv.Key, kv => kv.ToArray());

            foreach (var modelsPerTowar in modelsPerTowary)
            {
                Towar towar = modelsPerTowar.Key;
                OrderDocumentCreatorModel[] models = modelsPerTowar.Value;

                PozycjaDokHandlowego pozycja = new PozycjaDokHandlowego(dokumentHandlowy);
                _session.AddRow(pozycja);
                pozycja.Towar = towar;
                pozycja.Ilosc = models.Sum(x => x.Quantity);

                APSPozycjaDokHandlowegoExt pozycjaExt = pozycja.GetAPSExt();
                pozycjaExt.DeliveryDate = models.OrderBy(x => x.Term).FirstOrDefault().Term;

                foreach (OrderDocumentCreatorModel model in models)
                {
                    model.ExistsOrder = true;
                }
            }
        }

        private void CreatePositionsSeparately(DokumentHandlowy dokumentHandlowy)
        {
            foreach (OrderDocumentCreatorModel model in _models)
            {
                CreatePosition(dokumentHandlowy, model);
            }
        }

        private void CreatePosition(DokumentHandlowy dokumentHandlowy, OrderDocumentCreatorModel model)
        {
            PozycjaDokHandlowego pozycja = new PozycjaDokHandlowego(dokumentHandlowy);
            _session.AddRow(pozycja);
            pozycja.Towar = model.Towar;
            pozycja.Ilosc = model.Quantity;
            pozycja.Cena = model.PurchasePriceNetto;

            APSPozycjaDokHandlowegoExt pozycjaExt = pozycja.GetAPSExt();
            pozycjaExt.DeliveryDate = model.Term;

            model.ExistsOrder = true;
        }
    }
}
