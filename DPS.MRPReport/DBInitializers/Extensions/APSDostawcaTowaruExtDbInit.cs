using DPS.MRPReport.DBInitializers.Abstractions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Tables.Extensions;
using Soneta.Business;
using Soneta.Towary;
using System.Linq;

namespace DPS.MRPReport.DBInitializers.Extensions
{
	internal class APSDostawcaTowaruExtDbInit : DbInitBase
	{
        public APSDostawcaTowaruExtDbInit(ITransaction transaction) : base(transaction)
        {
        }

		internal override void Initialize()
        {
			DostawcaTowaru[] existingBaseObjectsWithoutExtension = _transaction
                .Session
                .GetTowary()
                .DostawcyTowaru
                .PrimaryKey[GetRowConditionForRowsWithoutExtensions()]
                .Cast<DostawcaTowaru>()
                .ToArray();

            foreach (var baseObject in existingBaseObjectsWithoutExtension)
            {
                var extension = new APSDostawcaTowaruExt(baseObject);
                _transaction.Session.AddRow(extension);
            }
        }

        private RowCondition GetRowConditionForRowsWithoutExtensions()
        {
            return new RowCondition.Not(
                new RowCondition.Exists(
                    nameof(APSDostTowExt),
                    nameof(DostawcaTowaru),
                        new FieldCondition.Null(nameof(DostawcaTowaru), false)
                    )
                );
        }
    }
}