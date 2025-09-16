using DPS.MRPReport.Enums;
using Soneta.Business;

namespace DPS.MRPReport.Rows
{
	public class APSReportMRPPositionRel : DPSMRPReportModule.APSReportMRPPositionRelRow
	{
		private bool _deleteWhitoutChild;

		public APSReportMRPPositionRel(RowCreator creator) : base(creator)
		{
		}

		public APSReportMRPPositionRel([Required] APSReportMRPPosition parent, [Required] APSReportMRPPosition child, [Required] APSReportMRPPositionRelTypeEnum relationtype)
			: base(parent, child, relationtype)
		{
		}

		protected override void OnDeleted()
		{
			if(!_deleteWhitoutChild)
			{
				Child.Delete();
			}
		}

		public void DeleteWhitoutChild()
		{
			_deleteWhitoutChild = true;
			base.Delete();
		}

		public override string ToString()
		{
			return $"{Parent.ToString()} --> {Child.ToString()}";
		}
	}
}
