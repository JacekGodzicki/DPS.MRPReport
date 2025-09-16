using DPS.MRPReport.Workers.Config.Abstractions;
using Soneta.Business;
using System;

namespace DPS.MRPReport.UI.Extenders.Config.Abstractions
{
    public class ConfigExtenderBase<T>
           where T : ConfigurationNodesManager
    {
        [Context]
        public Session Session { get; set; }

        public T ConfigWorker { get; set; }

        public ConfigExtenderBase(Session session)
        {
            if (session.IsConfig)
            {
                ConfigWorker = (T)Activator.CreateInstance(typeof(T), session);
            }
        }
    }
}
