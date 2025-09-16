using DPS.MRPReport.Workers.Extensions;
using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows.Extensions;
using Soneta.Business;
using Soneta.Magazyny;

[assembly: Worker(typeof(APSMagazynExtWorker), typeof(Magazyn))]

namespace DPS.MRPReport.Workers.Extensions
{
	public class APSMagazynExtWorker : ISessionable
    {
        private APSMagazynExt _extension;

        [Context]
        public Session Session { get; set; }

        [Context]
        public Magazyn ExtendedObject { get; set; }

        public APSMagazynExt Extension
        {
            get
            {
                if (ExtendedObject != null && _extension?.Magazyn != ExtendedObject)
                {
                    _extension = ExtendedObject.GetAPSExt();
                    if (_extension == null && !this.Session.ReadOnly)
                    {
                        var session = ExtendedObject.Session;
                        _extension = new APSMagazynExt(ExtendedObject);
                        using (ITransaction transaction = session.Logout(true))
                        {
                            session.AddRow(_extension);
                            transaction.CommitUI();
                        }
                    }
                }
                return _extension;
            }
        }
    }
}
