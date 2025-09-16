using DPS.MRPReport.Extensions;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.Workers.Extensions;
using Soneta.Business;
using Soneta.Towary;

[assembly: Worker(typeof(APSDostawcaTowaruExtWorker), typeof(DostawcaTowaru))]

namespace DPS.MRPReport.Workers.Extensions
{
	public class APSDostawcaTowaruExtWorker : ISessionable
    {
        private APSDostawcaTowaruExt _extension;

        [Context]
        public Session Session { get; set; }

        [Context]
        public DostawcaTowaru ExtendedObject { get; set; }

        public APSDostawcaTowaruExt Extension
        {
            get
            {
                if (ExtendedObject != null && _extension?.DostawcaTowaru != ExtendedObject)
                {
                    _extension = ExtendedObject.GetAPSExt();
                    if (_extension == null && !this.Session.ReadOnly)
                    {
                        var session = ExtendedObject.Session;
                        _extension = new APSDostawcaTowaruExt(ExtendedObject);
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