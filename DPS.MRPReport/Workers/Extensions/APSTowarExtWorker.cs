using DPS.MRPReport.Workers.Extensions;
using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows.Extensions;
using Soneta.Business;
using Soneta.Towary;

[assembly: Worker(typeof(APSTowarExtWorker), typeof(Towar))]

namespace DPS.MRPReport.Workers.Extensions
{
	public class APSTowarExtWorker : ISessionable
    {
        private APSTowarExt _extension;

        [Context]
        public Session Session { get; set; }

        [Context]
        public Towar ExtendedObject { get; set; }

        public APSTowarExt Extension
        {
            get
            {
                if (ExtendedObject != null && _extension?.Towar != ExtendedObject)
                {
                    _extension = ExtendedObject.GetAPSExt();
                    if (_extension == null && !this.Session.ReadOnly)
                    {
                        var session = ExtendedObject.Session;
                        _extension = new APSTowarExt(ExtendedObject);
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