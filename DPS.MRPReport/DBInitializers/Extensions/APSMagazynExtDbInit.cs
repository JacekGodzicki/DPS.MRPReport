using DPS.MRPReport.DBInitializers.Abstractions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Tables.Extensions;
using Soneta.Business;
using Soneta.Magazyny;
using System.Linq;

namespace DPS.MRPReport.DBInitializers.Extensions
{
	internal class APSMagazynExtDbInit : DbInitBase
	{
		public APSMagazynExtDbInit(ITransaction transaction) : base(transaction)
		{
		}

		internal override void Initialize()
        {
			Magazyn[] existingBaseObjectsWithoutExtension = _transaction
                .Session
                .GetMagazyny()
                .Magazyny
                .PrimaryKey[GetRowConditionForRowsWithoutExtensions()]
                .Cast<Magazyn>()
                .ToArray();

            foreach (var baseObject in existingBaseObjectsWithoutExtension)
            {
                var extension = new APSMagazynExt(baseObject);
                _transaction.Session.AddRow(extension);
            }
        }

		private RowCondition GetRowConditionForRowsWithoutExtensions()
        {
            return new RowCondition.Not(
                new RowCondition.Exists(
                    nameof(APSMagExt),
                    nameof(Magazyn),
                        new FieldCondition.Null(nameof(Magazyn), false)
                    )
                );
        }
    }
}
