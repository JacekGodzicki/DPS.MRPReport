using Soneta.Business;
using Soneta.Config;
using System;

namespace DPS.MRPReport.Workers.Config.Abstractions
{
    public abstract class ConfigurationNodesManager
    {
        public Context Context { get; set; }

        public Session Session { get; set; }

        private string MajorNode = "DPS";

        public virtual string MinorNode
            => throw new NotImplementedException("MinorNode musi zostać zaimplementowane poprzez override w klasie dziedziczącej po ConfigurationNodesManager");

        public void SetGuidedRowValue<T>(string name, T value)
            where T : GuidedRow
        {
            if (value is null)
            {
                SetValue(name, Guid.Empty, AttributeType._guid);
                return;
            }

            SetValue(name, value.Guid, AttributeType._guid);
        }

        public T GetGuidedRowValue<T>(GuidedTable table, string name)
            where T : GuidedRow
        {
            Guid guid = GetValue(name, Guid.Empty);
            if (guid == Guid.Empty)
            {
                return null;
            }

            try
            {
                return table[guid] as T;
            }
            catch
            {
                return null;
            }
        }

        public void SetValue<T>(string name, T value, AttributeType type)
        {
            SetValue(Session, MajorNode, MinorNode, name, value, type);
        }

        public T GetValue<T>(string name, T defaultValue)
        {
            return GetValue(Session, MajorNode, MinorNode, name, defaultValue);
        }

        private static void SetValue<T>(Session session, string majorNode, string minorNode, string name, T value, AttributeType type)
        {
            using (ITransaction transaction = session.Logout(true))
            {
                var cfgManager = new CfgManager(session);
                CfgNode node1 = cfgManager.Root.FindSubNode(majorNode, false) ?? cfgManager.Root.AddNode(majorNode, CfgNodeType.Node);
                CfgNode node2 = node1.FindSubNode(minorNode, false) ?? node1.AddNode(minorNode, CfgNodeType.Leaf);
                CfgAttribute attr = node2.FindAttribute(name, false);
                if (attr == null)
                {
                    node2.AddAttribute(name, type, value);
                }
                else
                {
                    attr.Value = value;
                }
                transaction.Commit();
            }
            session.InvokeChanged();
        }

        private static T GetValue<T>(Session session, string majorNode, string minorNode, string name, T def)
        {
            CfgManager cfgManager = new CfgManager(session);
            CfgNode node1 = cfgManager.Root.FindSubNode(majorNode, false);
            if (node1 == null)
            {
                return def;
            }

            CfgNode node2 = node1.FindSubNode(minorNode, false);
            if (node2 == null)
            {
                return def;
            }

            CfgAttribute attr = node2.FindAttribute(name, false);
            if (attr == null)
            {
                return def;
            }

            if (attr.Value == null)
            {
                return def;
            }
            return (T)attr.Value;
        }
    }
}
