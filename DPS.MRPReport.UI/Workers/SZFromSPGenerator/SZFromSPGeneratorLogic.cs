using DPS.MRPReport.Configurations;
using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.SZFromSPGenerator
{
	public class SZFromSPGeneratorLogic
	{
		private readonly APSReportMRPPosition _reportMRPPosition;
		private readonly Session _session;
		private readonly ConfigReportMRPMainWorker _configReportMRPMainWorker;

		public SZFromSPGeneratorLogic(APSReportMRPPosition reportMRPPosition)
		{
			_reportMRPPosition = reportMRPPosition;
			_session = reportMRPPosition.Session;
			_configReportMRPMainWorker = new ConfigReportMRPMainWorker(_session);
		}

		public void Generate()
		{
			SetDefinitionCodeSZ();
			if(_configReportMRPMainWorker.ChangeSPToSZRemoveMaterials
				&& _reportMRPPosition.ChildrenView.Any())
			{
				RemoveChildrenAndChildrenRelations();
				//_session.Saved += SessionSaved;
			}
		}

		private void SetDefinitionCodeSZ()
		{
			using(ITransaction transaction = _session.Logout(true))
			{
				_reportMRPPosition.DefinitionCode = APSReportMRPPositionConfiguration.DefinitionCode.SZ;

				transaction.CommitUI();
			}
		}

		private void RemoveChildrenAndChildrenRelations()
		{
			RemoveChildren();
			RemoveChildrenRelations();
		}

		private void RemoveChildren()
		{
			List<APSReportMRPPosition> children = _reportMRPPosition.Children
				.Where(x => !x.IsDeletedOrDetached())
				.ToList();

			using(ITransaction transaction = _session.Logout(true))
			{
				foreach(APSReportMRPPosition positionChild in children)
				{
					positionChild.Delete();
				}

				transaction.CommitUI();
			}
		}

		private void RemoveChildrenRelations()
		{
			List<APSReportMRPPositionRel> childrenRelations = _reportMRPPosition.ChildrenRelations
				.Where(x => !x.IsDeletedOrDetached())
				.ToList();

			using(ITransaction transaction = _session.Logout(true))
			{
				foreach(APSReportMRPPositionRel relation in childrenRelations)
				{
					relation.Delete();
				}

				transaction.CommitUI();
			}
		}

		//private void SessionSaved(object sender, EventArgs e)
		//{
		//	GenerateUpdatedRaportMRP();
		//}

		//private void GenerateUpdatedRaportMRP()
		//{
		//	using(Session session = _session.Login.CreateSession())
		//	{
		//		Date dateFrom = session.Global.Features.GetDate(FeatureConstants.Global_ReportMRPDataOstatniePrzeliczenie);
		//		ReportMRPGeneratorLogic generateRaportLogic = new ReportMRPGeneratorLogic(session, dateFrom);
		//		generateRaportLogic.Generate();

		//		session.Save();
		//	}
		//}
	}
}
