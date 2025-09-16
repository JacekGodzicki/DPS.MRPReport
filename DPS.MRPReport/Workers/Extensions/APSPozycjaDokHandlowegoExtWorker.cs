using DPS.MRPReport.Workers.Extensions;
using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows.Extensions;
using Soneta.Business;
using Soneta.Handel;

[assembly: Worker(typeof(APSPozycjaDokHandlowegoExtWorker), typeof(PozycjaDokHandlowego))]

namespace DPS.MRPReport.Workers.Extensions
{
	public class APSPozycjaDokHandlowegoExtWorker : ISessionable
    {
        private APSPozycjaDokHandlowegoExt _extension;

        [Context]
        public Session Session { get; set; }

        [Context]
        public PozycjaDokHandlowego ExtendedObject { get; set; }

        public APSPozycjaDokHandlowegoExt Extension
        {
            get
            {
                if (ExtendedObject != null && _extension?.PozycjaDokHandlowego != ExtendedObject)
                {
                    _extension = ExtendedObject.GetAPSExt();
                    if (_extension == null && !this.Session.ReadOnly)
                    {
                        var session = ExtendedObject.Session;
                        _extension = new APSPozycjaDokHandlowegoExt(ExtendedObject);
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
