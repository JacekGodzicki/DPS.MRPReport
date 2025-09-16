using DPS.MRPReport.DBInitializers.Abstractions;
using DPS.MRPReport.Install.Features;
using Soneta.Business;

namespace DPS.MRPReport.DBInitializers
{
	internal class FeaturesDbInit : DbInitBase
	{
		public FeaturesDbInit(ITransaction transaction) : base(transaction)
		{
		}

		internal override void Initialize()
		{
			using(Session session = _transaction.Session.Login.CreateSession(false, true))
			{
				InitializeGlobal(session);

				session.Save();
			}
		}

		private void InitializeGlobal(Session session)
		{
			new ReportMRPDataOstatniePrzeliczenieGlobalFeature(session).Create();
		}
	}
}
