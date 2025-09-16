using Soneta.Business.App;
using Soneta.Business;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using DPS.MRPReport.DBInitializers.Abstractions;
using DPS.MRPReport.DBInitializers;

[assembly: DatabaseInit("Konwersja danych dla Business",
						typeof(BusinessDbInit),
						"DPSMRPReport")]

namespace DPS.MRPReport.DBInitializers
{
    public class BusinessDbInit : IDatabaseInitializer
    {
        public void Initialize(Login login, int version)
        {
            Init(login, version, 999, nameof(BusinessDbInit), transaction =>
            {
                IEnumerable<Type> dbInitTypes = GetDbInitTypes();
                foreach(Type type in dbInitTypes)
                {
					DbInitBase instance = (DbInitBase)Activator.CreateInstance(type, [transaction]);
                    instance.Initialize();
				}
            }, true);
        }

        private IEnumerable<Type> GetDbInitTypes()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DbInitBase)) && !t.IsAbstract);
		}

        private void Init(Login login, int version, int xmlBusinessVersion, string sessionName, Action<ITransaction> action, bool createConfigSession = false)
        {
            if (version == 0 || version < xmlBusinessVersion)
            {
                using (Session session = login.CreateSession(false, createConfigSession, sessionName))
                {
                    using (ITransaction transaction = session.Logout(true))
                    {
                        action(transaction);
                        transaction.Commit();
                    }

                    session.Save();
                }
            }
        }
    }
}
