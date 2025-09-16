using DPS.MRPReport.DBInitializers.Abstractions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Tables.Extensions;
using Soneta.Business;
using Soneta.Towary;
using System.Linq;

namespace DPS.MRPReport.DBInitializers.Extensions
{
	internal class APSTowarExtDbInit : DbInitBase
    {
		public APSTowarExtDbInit(ITransaction transaction) : base(transaction)
		{
		}

		internal override void Initialize()
        {
			Towar[] existingBaseObjectsWithoutExtension = _transaction
                .Session
                .GetTowary()
                .Towary
                .PrimaryKey[GetRowConditionForRowsWithoutExtensions()]
                .Cast<Towar>()
                .ToArray();

            foreach (var baseObject in existingBaseObjectsWithoutExtension)
            {
                var extension = new APSTowarExt(baseObject);
                _transaction.Session.AddRow(extension);
            }
        }

        private RowCondition GetRowConditionForRowsWithoutExtensions()
        {
            return new RowCondition.Not(
                new RowCondition.Exists(
                    nameof(APSTowaryExt),
                    nameof(Towar),
                        new FieldCondition.Null(nameof(Towar), false)
                    )
                );
        }
    }
}
