using Soneta.Business;

namespace DPS.MRPReport.Extensions
{
    public static class RowExtension
    {
        public static bool IsDeletedOrDetached(this Row row)
        {
            return row.State == RowState.Deleted
                || row.State == RowState.Detached
                || row.Status.HasFlag(RowStatus.Deleting);
        }
    }
}