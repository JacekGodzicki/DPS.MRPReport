using DPS.MRPReport.Rows;
using DPS.MRPReport.Rows.Extensions;
using DPS.MRPReport.UI.Workers.ReportMRP.Models;
using Soneta.Business.UI;
using Soneta.Types;
using System.Collections.Generic;
using System.Linq;

namespace DPS.MRPReport.UI.Workers.ReportMRP
{
	public class ReportMRPGridGenerator
    {
        public class Params
        {
            public required IEnumerable<Date> Dates { get; set; }
            public required IEnumerable<DateParamsModel> SelectedDays { get; set; }
            public required string EditValuePropertyName { get; set; }
            public required string SelectedValuePropertyName { get; set; }
        }

        private readonly Date[] _dates;
        private readonly IEnumerable<DateParamsModel> _selectedDays;
        private readonly string _editValuePropertyName;
        private readonly string _selectedValuePropertyName;

        public ReportMRPGridGenerator(Params pars)
        {
            _dates = pars.Dates.ToArray();
            _selectedDays = pars.SelectedDays;
            _editValuePropertyName = pars.EditValuePropertyName;
            _selectedValuePropertyName = pars.SelectedValuePropertyName;
        }

        public UIElement Generate()
        {
            GridElement gridElement = CreateGridElement();

            IEnumerable<FieldElement> towarFieldElements = CreateTowarFieldElements();
            gridElement.Elements.AddRange(towarFieldElements);

            IEnumerable<FieldElement> dateFieldElements = CreateDateFieldElements();
            gridElement.Elements.AddRange(dateFieldElements);

            IEnumerable<FieldElement> summaryFieldElements = CreateSummaryFieldElements();
            gridElement.Elements.AddRange(summaryFieldElements);
            return gridElement;
        }


        private GridElement CreateGridElement()
        {
            return new GridElement()
            {
                Width = "*",
                Height = "*",
                EditValue = string.Format("{{{0}}}", _editValuePropertyName),
                SelectedValue = string.Format("{{{0}}}", _selectedValuePropertyName),
                SumType = CollectionSumType.None,
                EditInPlace = false,
                EditButton = CollectionButtonState.Visible,
                IsFilterRowVisible = true,
                OrderBy = "Towar.Kod"
			};
        }

        private IEnumerable<FieldElement> CreateTowarFieldElements()
        {
            List<FieldElement> fieldElements = new List<FieldElement>();
            FieldElement fieldElementCode = new FieldElement()
            {
                CaptionHtml = "Kod",
                EditValue = string.Format("{{{0}}}", $"{nameof(APSReportMRPElement.Towar)}.{nameof(APSReportMRPElement.Towar.Kod)}"),
                Width = "20"
            };
            fieldElements.Add(fieldElementCode);

            FieldElement fieldElementName = new FieldElement()
            {
                CaptionHtml = "Nazwa",
                EditValue = string.Format("{{{0}}}", $"{nameof(APSReportMRPElement.Towar)}.{nameof(APSReportMRPElement.Towar.Nazwa)}"),
				Width = "50"
            };
            fieldElements.Add(fieldElementName);

            FieldElement fieldElementUnit = new FieldElement()
            {
                CaptionHtml = "Jednostka",
                EditValue = string.Format("{{{0}}}", $"{nameof(APSReportMRPElement.Towar)}.{nameof(APSReportMRPElement.Towar.Jednostka)}.{nameof(APSReportMRPElement.Towar.Jednostka.Kod)}"),
				Width = "10"
            };
            fieldElements.Add(fieldElementUnit);

			FieldElement fieldElementObtainingMethod = new FieldElement()
			{
				CaptionHtml = "Metoda uzyskania",
				EditValue = string.Format("{{{0}}}", nameof(APSReportMRPElement.ObtainingMethod)),
				Width = "13"
			};
			fieldElements.Add(fieldElementObtainingMethod);

			FieldElement fieldElementDefaultTechnology = new FieldElement()
			{
				CaptionHtml = "Domyślna technologia",
				EditValue = string.Format("{{{0}}}", $"{nameof(APSReportMRPElement.Towar)}.Workers.{nameof(APSTowarExt)}.Extension.{nameof(APSTowarExt.ExistsDefaultTechnology)}"),
				Width = "15"
			};

			fieldElements.Add(fieldElementDefaultTechnology);
			return fieldElements;
        }

        private IEnumerable<FieldElement> CreateDateFieldElements()
        {
            List<FieldElement> fieldElements = new List<FieldElement>();
            FieldElement fieldElementTerminated = CreateTerminatedFieldElement();
			fieldElements.Add(fieldElementTerminated);

			for (int i = 0; i < _dates.Length - 1; i++)
            {
                Date date = _dates[i];
                FieldElement fieldElementDate = CreateDateFieldElement(date, i + 1);
                fieldElements.Add(fieldElementDate);
            }
            return fieldElements;
        }

		private FieldElement CreateTerminatedFieldElement()
        {
			FieldElement fieldElement = new FieldElement()
			{
				CaptionHtml = "Przeterminowane",
				EditValue = "{QuantityDates[0].Quantity}",
				Width = "12"
			};

            if(_selectedDays.Any(x => x.DateStr == "Przeterminowane"))
            {
				AppearanceElement appearanceElement = new AppearanceElement
				{
					BackColor = "#76A300",
					Condition = ""
				};

				fieldElement.Appearances.Add(appearanceElement);
			}
            return fieldElement;
		}

		private FieldElement CreateDateFieldElement(Date date, int index)
        {
            FieldElement fieldElementDate = new FieldElement()
            {
                CaptionHtml = date.ToString(),
                EditValue = string.Format("{{QuantityDates[{0}].Quantity}}", index),
                Width = "10"
            };

            if (_selectedDays.Any(x => x.GetDate() == date))
            {
                AppearanceElement appearanceElement = new AppearanceElement
                {
                    BackColor = "#76A300",
                    Condition = ""
                };

                fieldElementDate.Appearances.Add(appearanceElement);
            }
            return fieldElementDate;
        }

        private IEnumerable<FieldElement> CreateSummaryFieldElements()
        {
            List<FieldElement> fieldElements = new List<FieldElement>();
            FieldElement fieldElementSaldoKoncowe = new FieldElement()
            {
                CaptionHtml = "Saldo końcowe",
                EditValue = string.Format("{{{0}}}", nameof(APSReportMRPElement.FinalBalance)),
				Width = "10"
            };
            fieldElements.Add(fieldElementSaldoKoncowe);

            FieldElement fieldElementNadwyzka = new FieldElement()
            {
                CaptionHtml = "Nadwyżka",
                EditValue = string.Format("{{{0}}}", nameof(APSReportMRPElement.Overcapacity)),
				Width = "10"
            };
            fieldElements.Add(fieldElementNadwyzka);
            return fieldElements;
        }
    }
}
