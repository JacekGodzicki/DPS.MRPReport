using Soneta.Types;

namespace DPS.MRPReport.Enums
{
    public enum OrderingPolicyEnum
	{
        [Caption("Wymagana ilość netto")]
        RequiredQuantityNetto,
        [Caption("Stała ilość")]
        FixedQuantity,
		[Caption("Zapas minimalny")]
		MinimumReserve,
        [Caption("Kombinacja")]
		Combination
	}
}