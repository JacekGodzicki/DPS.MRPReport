using Soneta.Business;
using Soneta.ProdukcjaPro;
using System;
using Soneta.Types;
using System.Linq;
using DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersCreator.Models;
using DPS.MRPReport.Workers.Config;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Configurations;
using DPS.MRPReport.Extensions;
using DPS.MRPReport.UI.Workers.ReportMRPGenerator;

namespace DPS.MRPReport.UI.Workers.ProductionOrdersGenerators.ProductionOrdersGenerator
{
	public class ProductionOrdersGeneratorLogic
	{
		public class Params
		{
			public ProWydzial Wydzial { get; set; }
			public TowarModelContextBase Model { get; set; }
		}

		private readonly Session _session;
		private readonly ProWydzial _wydzial;
		private readonly TowarModelContextBase _model;
		private readonly ConfigReportMRPMainWorker _configReportMRPMainWorker;

		private ProZlecenie _zlecenie;

		public ProductionOrdersGeneratorLogic(Session session, Params pars)
		{
			_session = session;
			_wydzial = pars.Wydzial;
			_model = pars.Model;
			_configReportMRPMainWorker = new ConfigReportMRPMainWorker(session);

			_session.Changed += SessionChanged;
			_session.Disposed += SessionDisposed;
			_session.Saved += SessionSaved;
		}

		public ProZlecenie Create()
		{
			_zlecenie = CreateProZlecenie();
			return _zlecenie;
		}

		private ProZlecenie CreateProZlecenie()
		{
			TimeSpan timeSpan = new TimeSpan(_model.Technologia.Czas.Hours, _model.Technologia.Czas.Minutes, _model.Technologia.Czas.Seconds);
			DateTime dataRozpoczecia = (DateTime)_model.Term - timeSpan;

			using(ITransaction transaction = _session.Logout(true))
			{
				_zlecenie = new ProZlecenie();
				_session.AddRow(_zlecenie);
				_zlecenie.Wydzial = _wydzial;
				_zlecenie.Technologia = _model.Technologia;
				_zlecenie.Ilosc = _model.Quantity;
				_zlecenie.DataRozpoczecia = dataRozpoczecia;
				_zlecenie.CzasRozpoczecia = new TimeSec(dataRozpoczecia);
				_zlecenie.DataZakonczenia = _model.Term;

				transaction.CommitUI();
			}

			_model.ExistsZP = true;
			return _zlecenie;
		}

		private void SessionChanged(object sender, EventArgs e)
		{
			if(_zlecenie is null)
			{
				return;
			}

			if(_zlecenie.IsDeletedOrDetached())
			{
				SetModelExistsZPFalse();
				_session.Changed -= SessionChanged;
			}
		}

		private void SetModelExistsZPFalse()
		{
			_model.ExistsZP = false;
		}

		private void SessionDisposed(object sender, EventArgs e)
		{
			_session.Login.SetValue(typeof(DateTime), "ReportMRPUIParamsLoadDateTime", DateTime.Now);
		}

		private void SessionSaved(object sender, EventArgs e)
		{
			if(_zlecenie is null || _zlecenie.IsDeletedOrDetached())
			{
				return;
			}

			ConfigReportMRPMainWorker configWorker = new ConfigReportMRPMainWorker(_session);
			if(configWorker.RecalculateAfterCreatingDocumentOrOrder)
			{
				GenerateUpdatedRaportMRP();
			}
			else
			{
				UpdateSuggestionPositions();
			}
		}

		private void GenerateUpdatedRaportMRP()
		{
			using(Session session = _session.Login.CreateSession())
			{
				Date dateFrom = session.Global.Features.GetDate(FeatureConstants.FeatureNamesGlobal.ReportMRPDataOstatniePrzeliczenie);
				new ReportMRPGeneratorLogic(session, dateFrom).Generate();

				session.Save();
			}
		}

		private void UpdateSuggestionPositions()
		{
			using(Session newSession = _session.Login.CreateSession())
			{
				using(ITransaction transaction = newSession.Logout(true))
				{
					APSReportMRPPosition[] positionsSession = _model.SuggestionPositions
						.Select(x => newSession.Get(x))
						.ToArray();

					foreach(APSReportMRPPosition position in positionsSession)
					{
						position.DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.ZP;
						position.DocumentNumber = _zlecenie.Numer.NumerPelny;
						position.CreatedFromSuggestion = true;
						position.RelatedSuggestionParentRel.DeleteWhitoutChild();
						UpdateRelatedPositions(position);
					}

					transaction.Commit();
				}

				newSession.Save();
			}
		}

		private void UpdateRelatedPositions(APSReportMRPPosition position)
		{
			foreach(APSReportMRPPosition child in position.Children)
			{
				child.DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.PR;
				child.DocumentNumber = _zlecenie.Numer.NumerPelny;
				child.CreatedFromSuggestion = true;
			}
		}
	}
}
