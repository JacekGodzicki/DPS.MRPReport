using Soneta.Business;

namespace DPS.MRPReport.Install
{
	public class FeatureDefinitionParams
	{
		public FeatureDefinitionParams()
		{
			Algorithm = FeatureAlgorithm.DB;
			TypeNumber = FeatureTypeNumber.String;
			ReadOnlyMode = FeatureReadOnlyMode.Standard;
			Description = string.Empty;
			Name = string.Empty;
			TableName = string.Empty;
			Code = string.Empty;
			GeneratedCode = string.Empty;
			Dictionary = string.Empty;
			Category = string.Empty;
		}

		public Session Session { get; set; }
		public Table ReferenceTable { get; set; }
		public FeatureAlgorithm Algorithm { get; set; }
		public FeatureTypeNumber TypeNumber { get; set; }
		public FeatureReadOnlyMode ReadOnlyMode { get; set; }
		public string Description { get; set; }
		public string Name { get; set; }
		public string TableName { get; set; }
		public string Code { get; set; }
		public string GeneratedCode { get; set; }
		public string Dictionary { get; set; }
		public bool IsDictionary { get; set; }
		public bool StrictDictionary { get; set; }
		public bool Group { get; set; }
		public bool IsInitValue { get; set; }
		public FeatureRequiredModes ValueRequiredMode { get; set; }
		public string Category { get; set; }
		public bool History { get; set; }
		public object InitValue { get; set; }
	}
}
