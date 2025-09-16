using System.Collections.Generic;
using Soneta.Business;
using Soneta.CRM;
using Soneta.Towary;
using Soneta.Types;
using System.Linq;
using DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator.Models;
using DPS.MRPReport.Rows;
using DPS.MRPReport.Models;
using DPS.MRPReport.Utils;

namespace DPS.MRPReport.UI.Workers.DocumentsGenerators.OrderDocument.OrderDocumentsCreator
{
    public class OrderDocumentsCreatorLogic
    {
        public class Params
        {
            public Date[] SelectedDates { get; set; }
            public APSReportMRPElement[] ReportMRPElements { get; set; }
        }

        private readonly Context _context;
        private readonly Session _session;
        private readonly Date[] _selectedDates;
        private readonly APSReportMRPElement[] _reportMRPElements;

        public OrderDocumentsCreatorLogic(Context context, Params pars)
        {
            _context = context;
            _session = context.Session;
            _selectedDates = pars.SelectedDates;
            _reportMRPElements = pars.ReportMRPElements;
        }

        public OrderDocumentCreatorModel[] GetTowarModels()
        {
            List<OrderDocumentCreatorModel> models = new List<OrderDocumentCreatorModel>();
            foreach (APSReportMRPElement pozycja in _reportMRPElements)
            {
                List<QuantityDateModel> ilosciPerDatyDodatnie = pozycja.QuantityDates
                    .Where(x => _selectedDates.Contains(x.StartDate) && x.Quantity.IsPlus)
                    .ToList();

                foreach (QuantityDateModel iloscPerData in ilosciPerDatyDodatnie)
                {
					Date term = iloscPerData.AvailabilityDate;
					if(term < Date.Today)
					{
						term = Date.Today;
					}

					OrderDocumentCreatorModel.ModelCreator modelCreator = new OrderDocumentCreatorModel.ModelCreator
                    {
                        Towar = pozycja.Towar,
                        Term = term,
                        Quantity = iloscPerData.Quantity,
                        Supplier = GetSupplier(pozycja.Towar),
                        SuggestionPositions = iloscPerData.SuggestionPositions
					};

                    OrderDocumentCreatorModel model = new OrderDocumentCreatorModel(_context, modelCreator);
                    models.Add(model);
                }
            }

            return models.ToArray();
        }

        private Kontrahent GetSupplier(Towar towar)
        {
            return new DostawcaTowaruUtil(_session).GetDefaultDostawcaKontrahent(towar);
        }
    }
}
