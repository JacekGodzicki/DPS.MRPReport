using Soneta.Business;

namespace DPS.MRPReport.DBInitializers.Abstractions
{
	public abstract class DbInitBase
	{
		protected readonly ITransaction _transaction;

		public DbInitBase(ITransaction transaction)
		{
			this._transaction = transaction;
		}

		internal abstract void Initialize();
	}
}
