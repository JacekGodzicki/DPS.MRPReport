using DPS.MRPReport.Utils;
using Soneta.Business;
using System;

namespace DPS.MRPReport.Install
{
	public class FeatureDefinitionCreator
	{
		private readonly Session _session;
		private readonly FeatureDefinitionParams _featureDefinitionParams;
		private readonly FeatureDefinitionUtil _featureDefinitionUtil;

		public FeatureDefinitionCreator(FeatureDefinitionParams featureDefinitionParams)
		{
			_featureDefinitionParams = featureDefinitionParams;
			_session = featureDefinitionParams.Session;
			_featureDefinitionUtil = new FeatureDefinitionUtil(_session);
		}

		public void CreateOrUpdate()
		{
			FeatureDefinition featureDefinition = _featureDefinitionUtil
				.FindFeatureDefinitionByName(_featureDefinitionParams.Name, _featureDefinitionParams.TableName);

			if(featureDefinition is not null)
			{
				UpdateFeatureDefinition(featureDefinition);
				return;
			}
			CreateFeatureDefinition();
		}

		private void UpdateFeatureDefinition(FeatureDefinition featureDefinition)
		{
			using(ITransaction transaction = _featureDefinitionParams.Session.Logout(true))
			{
				bool featureDefinitionUpdated = false;
				if(featureDefinition.TypeNumber == FeatureTypeNumber.Reference && featureDefinition.ReferenceTable != _featureDefinitionParams.ReferenceTable)
				{
					featureDefinition.ReferenceTable = _featureDefinitionParams.ReferenceTable;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.Algorithm != _featureDefinitionParams.Algorithm)
				{
					featureDefinition.Algorithm = _featureDefinitionParams.Algorithm;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.TypeNumber != _featureDefinitionParams.TypeNumber)
				{
					featureDefinition.TypeNumber = _featureDefinitionParams.TypeNumber;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.ReadOnlyMode != _featureDefinitionParams.ReadOnlyMode)
				{
					featureDefinition.ReadOnlyMode = _featureDefinitionParams.ReadOnlyMode;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.Description != _featureDefinitionParams.Description)
				{
					featureDefinition.Description = _featureDefinitionParams.Description;
					featureDefinitionUpdated = true;
				}

				if(_featureDefinitionParams.Algorithm != FeatureAlgorithm.DB && (featureDefinition.Code == null || featureDefinition.Code.ToString() != _featureDefinitionParams.Code))
				{
					featureDefinition.Code = _featureDefinitionParams.Code;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.Dictionary != _featureDefinitionParams.Dictionary)
				{
					featureDefinition.Dictionary = _featureDefinitionParams.Dictionary;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.StrictDictionary != _featureDefinitionParams.StrictDictionary)
				{
					featureDefinition.StrictDictionary = _featureDefinitionParams.StrictDictionary;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.Group != _featureDefinitionParams.Group)
				{
					featureDefinition.Group = _featureDefinitionParams.Group;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.IsDictionary != _featureDefinitionParams.IsDictionary)
				{
					featureDefinition.IsDictionary = _featureDefinitionParams.IsDictionary;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.IsInitValue != _featureDefinitionParams.IsInitValue)
				{
					featureDefinition.IsInitValue = _featureDefinitionParams.IsInitValue;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.ValueRequiredMode != _featureDefinitionParams.ValueRequiredMode)
				{
					featureDefinition.ValueRequiredMode = _featureDefinitionParams.ValueRequiredMode;
					featureDefinitionUpdated = true;
				}

				if(featureDefinition.History != _featureDefinitionParams.History)
				{
					featureDefinition.History = _featureDefinitionParams.History;
					featureDefinitionUpdated = true;
				}

				featureDefinitionUpdated = UpdateInitValue(featureDefinition, typeof(string), string.Empty) ? true : featureDefinitionUpdated;
				featureDefinitionUpdated = UpdateInitValue(featureDefinition, typeof(int), 0) ? true : featureDefinitionUpdated;
				featureDefinitionUpdated = UpdateInitValue(featureDefinition, typeof(bool), false) ? true : featureDefinitionUpdated;

				if(featureDefinitionUpdated)
				{
					transaction.CommitUI();
				}
			}
		}

		public bool UpdateInitValue(FeatureDefinition featureDefinition, Type type, object defValue)
		{
			bool updated = false;
			Type initValueParams = _featureDefinitionParams.InitValue != null ? _featureDefinitionParams.InitValue.GetType() : null;
			if(featureDefinition.InitValue != null && featureDefinition.InitValue.GetType() == type)
			{
				if(initValueParams != null && initValueParams == type)
				{
					featureDefinition.InitValue = _featureDefinitionParams.InitValue;
					updated = true;
				}
				else
				{
					if(featureDefinition.InitValue is string && featureDefinition.InitValue as string != defValue as string)
					{
						featureDefinition.InitValue = defValue;
						updated = true;
					}
					if(featureDefinition.InitValue is bool && bool.Parse(featureDefinition.InitValue.ToString()) != bool.Parse(defValue.ToString()))
					{
						featureDefinition.InitValue = defValue;
						updated = true;
					}
					if(featureDefinition.InitValue is int && int.Parse(featureDefinition.InitValue.ToString()) != int.Parse(defValue.ToString()))
					{
						featureDefinition.InitValue = defValue;
						updated = true;
					}
				}
			}

			return updated;
		}

		private void CreateFeatureDefinition()
		{
			using(ITransaction transaction = _session.Logout(true))
			{
				FeatureDefinition featureDefinition = new FeatureDefinition(_featureDefinitionParams.TableName);
				featureDefinition.Group = _featureDefinitionParams.Group;
				featureDefinition.Name = _featureDefinitionParams.Name;
				featureDefinition.Algorithm = _featureDefinitionParams.Algorithm;
				featureDefinition.TypeNumber = _featureDefinitionParams.TypeNumber;
				featureDefinition.ReadOnlyMode = _featureDefinitionParams.ReadOnlyMode;
				featureDefinition.Description = _featureDefinitionParams.Description;
				featureDefinition.StrictDictionary = _featureDefinitionParams.StrictDictionary;
				featureDefinition.IsDictionary = _featureDefinitionParams.IsDictionary;
				featureDefinition.IsInitValue = _featureDefinitionParams.IsInitValue;
				featureDefinition.ValueRequiredMode = _featureDefinitionParams.ValueRequiredMode;
				featureDefinition.History = _featureDefinitionParams.History;
				if(_featureDefinitionParams.ReferenceTable != null)
				{
					featureDefinition.ReferenceTable = _featureDefinitionParams.ReferenceTable;
				}
				if(_featureDefinitionParams.Algorithm != FeatureAlgorithm.DB && !string.IsNullOrEmpty(_featureDefinitionParams.Code))
				{
					featureDefinition.Code = _featureDefinitionParams.Code;
				}
				_session.AddRow(featureDefinition);
				transaction.CommitUI();
			}
		}

	}
}
