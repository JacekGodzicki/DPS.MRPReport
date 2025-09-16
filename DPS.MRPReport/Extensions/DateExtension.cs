using Soneta.Types;
using System;

namespace DPS.MRPReport.Extensions
{
	public static class DateExtension
	{
		public static Date AddBusinessDays(this Date date, int value)
		{
			int factor = value > 0 ? 1 : -1;
			int valueAbs = int.Abs(value);
			Date finalDate = date;
			int i = 0;
			while(i < valueAbs)
			{
				finalDate = finalDate.AddDays(factor);
				if(!Date.FreeOrHoliday(finalDate))
				{
					i++;
				}
			}
			return finalDate;
		}

		public static DateOnly ToDateOnly(this Date date)
		{
			return new DateOnly(date.Year, date.Month, date.Day);
		}
	}
}
