using DPS.MRPReport.Configurations;
using Soneta.Business;
using Soneta.Config;

namespace DPS.MRPReport.Install.Features
{
	public class ReportMRPDataOstatniePrzeliczenieGlobalFeature
	{
		private readonly ISessionable _sessionable;

		public ReportMRPDataOstatniePrzeliczenieGlobalFeature(ISessionable sessionable)
		{
			this._sessionable = sessionable;
		}

		public void Create()
		{
			FeatureDefinitionParams featureDefinitionParams = new FeatureDefinitionParams
			{
				Algorithm = FeatureAlgorithm.DB,
				ReadOnlyMode = FeatureReadOnlyMode.Standard,
				Name = FeatureConstants.FeatureNamesGlobal.ReportMRPDataOstatniePrzeliczenie,
				TableName = nameof(CfgNodes),
				Session = _sessionable.Session,
				TypeNumber = FeatureTypeNumber.Date
			};
			new FeatureDefinitionCreator(featureDefinitionParams).CreateOrUpdate();
		}
	}
}
