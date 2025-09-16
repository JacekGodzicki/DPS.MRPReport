using Soneta.Handel;
using System;
using System.Linq;

namespace DPS.MRPReport.Extensions
{
	public static class DokumentHandlowyExtension
	{
		public static bool IsZatwierdzonyOrZablokowany(this DokumentHandlowy dokumentHandlowy)
		{
			StanDokumentuHandlowego[] statesZatwierdzonyZablokowany = [StanDokumentuHandlowego.Zatwierdzony, StanDokumentuHandlowego.Zablokowany];
			return statesZatwierdzonyZablokowany.Contains(dokumentHandlowy.Stan);
		}
	}
}
