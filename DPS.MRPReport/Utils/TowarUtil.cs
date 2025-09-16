using Soneta.CRM;
using Soneta.Magazyny;
using Soneta.Towary;
using System.Reflection;

namespace DPS.MRPReport.Utils
{
    public class TowarUtil
    {
        private readonly Towar _towar;

        public TowarUtil(Towar towar)
        {
            _towar = towar;
        }

        /// <summary>
        /// Pobiera ostatnią cenę zakupu z faktury zakupu tj. ZK. Nie uwzględnia cen z PZ.
        /// </summary>
        /// <param name="towar"></param>
        /// <param name="kontrahent"></param>
        /// <returns></returns>
        public ICena GetLastPurchasePrice(Kontrahent kontrahent)
        {
            OstatniaCenaWorker.OstatniaCenaParams pars = new OstatniaCenaWorker.OstatniaCenaParams(_towar)
            {
                Kierunek = KierunekPartii.Przychód,
                Kontrahent = kontrahent
            };
            return GetLastPrice(pars);
        }

        private ICena GetLastPrice(OstatniaCenaWorker.OstatniaCenaParams pars)
        {
            MethodInfo methodInfo = typeof(Towar).GetMethod("GetOstatniaCena", BindingFlags.Instance | BindingFlags.NonPublic, [typeof(OstatniaCenaWorker.OstatniaCenaParams)]);
            return methodInfo.Invoke(_towar, [pars]) as ICena;
        }
    }
}