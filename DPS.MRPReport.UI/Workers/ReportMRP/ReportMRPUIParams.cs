using DPS.MRPReport.Configurations;
using DPS.MRPReport.Enums;
using DPS.MRPReport.UI.Workers.ReportMRP.Models;
using DPS.MRPReport.Workers.Config;
using Soneta.Business;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.ReportMRP
{
	public class ReportMRPUIParams : ContextBase
    {
        public enum ReportMRPTypeEnum
        {
            [Caption("Wszystkie")]
            All,
			[Caption("Wszystkie sugestie")]
			AllSuggestions,
			[Caption("Sugestie zakupu")]
			PurchasingSuggestions,
			[Caption("Sugestie produkcji")]
			ProductionSuggestions
		}

        private readonly int _numberOfDaysAheadForCalc;

		private Date _actualDate;
        private GroupingAccuracyEnum _groupingAccuracy = GroupingAccuracyEnum.Months;
		private ReportMRPTypeEnum _reportMRPType = ReportMRPTypeEnum.All;
		private int _numberOfColumns = 10;
		private DateParamsModel[] _selectedDates = [];

        public ReportMRPUIParams(Context context) : base(context)
        {
            _actualDate = Date.Today;
			ConfigReportMRPMainWorker configReportMRPMainWorker = new ConfigReportMRPMainWorker(context.Session);
            _numberOfDaysAheadForCalc = configReportMRPMainWorker.NumberOfDaysAheadForCalc;
			Initialize();
        }

		[Caption("Data ostatnie przeliczenie")]
        public Date LastCalculationDate
            => Session.Global.Features.GetDate(FeatureConstants.FeatureNamesGlobal.ReportMRPDataOstatniePrzeliczenie);

        [Caption("Rodzaj")]
        public ReportMRPTypeEnum ReportMRPType
        {
            get => _reportMRPType;
            set
            {
                _reportMRPType = value;
				SaveProperty(nameof(ReportMRPType));
				OnChanged();
            }
        }

        [Caption("Dokładność grupowania")]
        public GroupingAccuracyEnum GroupingAccuracy
        {
            get => _groupingAccuracy;
            set
            {
                _groupingAccuracy = value;
                SelectedDates = [];
				SaveProperty(nameof(GroupingAccuracy));
				OnChanged();
            }
        }

        [Caption("Dokladność grupowania - liczba kolumn")]
        public int NumberOfColumns
        {
            get => _numberOfColumns;
            set
            {
                if (value < 1)
                {
                    _numberOfColumns = 1;
                }
                else if(_numberOfDaysAheadForCalc != 0 && value > _numberOfDaysAheadForCalc)
                {
                    _numberOfColumns = _numberOfDaysAheadForCalc;
				}
                else
                {
                    _numberOfColumns = value;
                }
                SelectedDates = [];
				SaveProperty(nameof(NumberOfColumns));
				OnChanged();
            }
        }

        [Caption("Wybrane dni")]
        public DateParamsModel[] SelectedDates
        {
            get => _selectedDates;
            set
            {
                _selectedDates = value;
                SaveProperty(nameof(SelectedDates));
                OnChanged();
            }
        }

        public object GetListSelectedDates()
        {
            List<DateParamsModel> dateModels = GetDateParamsModels();
            LookupInfo lookupInfo = new LookupInfo(new LookupInfo.EnumerableItem(
                string.Empty,
                dateModels,
                [
                    new PathPropertyInfo(typeof(DateParamsModel), nameof(DateParamsModel.DateStr))
                ]
            ));
            lookupInfo.ComboBox = false;
            return lookupInfo;
        }

        public Date[] GetGroupDates()
        {
            List<Date> dates = [Date.Today];
            for (int i = 1; i <= NumberOfColumns; i++)
            {
                Date date;
                switch (GroupingAccuracy)
                {
                    case GroupingAccuracyEnum.Months:
                        date = Date.Today.AddMonths(i);
                        break;
                    case GroupingAccuracyEnum.Weeks:
                        date = Date.Today.AddDays(i * 7);
                        break;
                    default:
                        date = Date.Today.AddDays(i);
                        break;
                }
                dates.Add(date);
            }
            return dates.ToArray();
        }

        public Date[] GetSelectedDates()
        {
            return SelectedDates
                .Select(x => x.GetDate())
                .ToArray();
        }

        private void Initialize()
        {
			DateTime value = Session.Login.GetValue("ReportMRPUIParamsLoadDateTime", () => DateTime.MinValue);
            TimeSpan timeSpan = DateTime.Now - value;

			if(timeSpan.TotalSeconds < 1)
            {
                //LoadPropertiesValue();
				Session.Login.RemoveValue(typeof(DateTime), "ReportMRPUIParamsLoadDateTime");
			}
        }

		private void LoadPropertiesValue()
		{
			if(LoadProperty(nameof(ReportMRPType)) is ReportMRPTypeEnum reportMRPType)
			{
				_reportMRPType = reportMRPType;
			}

			if(LoadProperty(nameof(GroupingAccuracy)) is GroupingAccuracyEnum dokladnoscGrupowania)
			{
				_groupingAccuracy = dokladnoscGrupowania;
			}

			if(LoadProperty(nameof(NumberOfColumns)) is int liczbaKolumn)
			{
				_numberOfColumns = liczbaKolumn;
			}

			if(LoadProperty(nameof(SelectedDates)) is DateParamsModel[] wybraneDni)
			{
				_selectedDates = wybraneDni;
			}
		}

		private List<DateParamsModel> GetDateParamsModels()
        {
            Date dateTomorrow = Date.Today.AddDays(-1);
			List<DateParamsModel> dateModels = [new DateParamsModel("Przeterminowane", dateTomorrow)];
            Date dateStart = Date.Today;
            for (int i = 0; i < _numberOfColumns; i++)
            {
                Date data = this.GroupingAccuracy switch
                {
					GroupingAccuracyEnum.Weeks => dateStart.AddDays(i * 7),
					GroupingAccuracyEnum.Months => dateStart.AddMonths(i),
                    _ => dateStart.AddDays(i)
                };

                DateParamsModel dzienModel = new DateParamsModel(data);
                dateModels.Add(dzienModel);
            }
            return dateModels;
        }
    }
}