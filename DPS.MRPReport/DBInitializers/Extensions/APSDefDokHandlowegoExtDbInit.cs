using DPS.MRPReport.DBInitializers.Abstractions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Tables.Extensions;
using Soneta.Business;
using Soneta.Handel;
using System.Linq;

namespace DPS.MRPReport.DBInitializers.Extensions
{
	internal class APSDefDokHandlowegoExtDbInit : DbInitBase
	{
        public APSDefDokHandlowegoExtDbInit(ITransaction transaction) : base(transaction)
        {
        }

		internal override void Initialize()
		{
			DefDokHandlowego[] existingBaseObjectsWithoutExtension = _transaction
				.Session
				.GetHandel()
				.DefDokHandlowych
				.PrimaryKey[GetRowConditionForRowsWithoutExtensions()]
				.Cast<DefDokHandlowego>()
				.ToArray();

			foreach(var baseObject in existingBaseObjectsWithoutExtension)
			{
				var extension = new APSDefDokHandlowegoExt(baseObject);
				_transaction.Session.AddRow(extension);
			}
		}

		private RowCondition GetRowConditionForRowsWithoutExtensions()
        {
            return new RowCondition.Not(
                new RowCondition.Exists(
                    nameof(APSDefDHExt),
                    nameof(DefDokHandlowego),
                        new FieldCondition.Null(nameof(DefDokHandlowego), false)
                    )
                );
        }
    }
}
