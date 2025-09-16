using Soneta.Business;
using Soneta.Business.Db;

namespace DPS.MRPReport.Utils
{
	public class FeatureDefinitionUtil
	{
		private readonly Session _session;

		public FeatureDefinitionUtil(Session session)
		{
			_session = session;
		}

		public bool Exists(string featurName, string tableName)
		{
			RowCondition rc = new FieldCondition.Equal(nameof(FeatureDefinition.Name), featurName);
			rc &= new FieldCondition.Equal(nameof(FeatureDefinition.TableName), tableName);
			return _session.GetBusiness().FeatureDefs.ByName[rc].Any;
		}

		public FeatureDefinition FindFeatureDefinitionByName(string featurName, string tableName)
		{
			RowCondition rc = new FieldCondition.Equal(nameof(FeatureDefinition.Name), featurName);
			rc &= new FieldCondition.Equal(nameof(FeatureDefinition.TableName), tableName);
			return _session.GetBusiness().FeatureDefs.ByName[rc].GetFirst();
		}
	}
}
