using Soneta.Types;

namespace DPS.MRPReport.UI.Workers.ReportMRP.Models
{
	public class DateParamsModel
	{
		private readonly Date _date;
		private readonly string _label;

		public DateParamsModel(string label, Date date)
		{
			_label = label;
			_date = date;
		}

		public DateParamsModel(Date date)
		{
			_date = date;
			_label = date.ToString();
		}

		public string DateStr => _label;

		public Date GetDate()
		{
			return _date;
		}

		public override string ToString()
		{
			return DateStr;
		}
	}
}