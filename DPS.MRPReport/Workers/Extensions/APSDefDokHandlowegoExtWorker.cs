using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Workers.Extensions;
using Soneta.Business;
using Soneta.Handel;

[assembly: Worker(typeof(APSDefDokHandlowegoExtWorker), typeof(DefDokHandlowego))]

namespace DPS.MRPReport.Workers.Extensions
{
	public class APSDefDokHandlowegoExtWorker : ISessionable
    {
        private APSDefDokHandlowegoExt _extension;

        [Context]
        public Session Session { get; set; }

        [Context]
        public DefDokHandlowego ExtendedObject { get; set; }

        public APSDefDokHandlowegoExt Extension
        {
            get
            {
                if (ExtendedObject != null && _extension?.DefDokHandlowego != ExtendedObject)
                {
                    _extension = ExtendedObject.GetAPSExt();
                    if (_extension == null && !this.Session.ReadOnly)
                    {
                        var session = ExtendedObject.Session;
                        _extension = new APSDefDokHandlowegoExt(ExtendedObject);
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
