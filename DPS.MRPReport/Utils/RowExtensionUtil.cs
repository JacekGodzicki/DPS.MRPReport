using Soneta.Business;
using System;

namespace DPS.MRPReport.Utils
{
    /// <summary>
	/// Klasa pomocnicza do rozszerzania standardowych tabel, utworzonych przez Sonetę
	/// </summary>
	/// <typeparam name="TRow"></typeparam>
	/// <typeparam name="TRowExt"></typeparam>
    public class RowExtensionUtil<TRow, TRowExt>
        where TRow : Row
        where TRowExt : Row
    {
        /// <summary>
		/// Metoda przyśpieszająca tworzenie rozszerzenia dziedziczącego po interfejsie ICustomRowExtension
		/// </summary>
		/// <param name="row"></param>
        public void CreateExtension(Row row)
        {
            Session session = row.Session;
            TRowExt extension = (TRowExt)Activator.CreateInstance(typeof(TRowExt), row);
            session.AddRow(extension);
        }

        /// <summary>
		/// Metoda blokująca możliwość usunięcia rozszerzenia dziedziczącego
		/// po interfejsie ICustomRowExtension bez usunięcia najpierw obiektu rozszerzanego.
		/// </summary>
		/// <param name="extendedRow"></param>
		/// <param name="rowExtension"></param>
		/// <exception cref="RowException"></exception>
        public void DeletingWithoutExtendedObjectNotAllowed(Row extendedRow, Row rowExtension)
        {
            string message = "Nie można skasować rozszerzenia bez kasowania obiektu rozszerzanego.";
            DeletingWithoutReferencedObjectNotAllowed(extendedRow, rowExtension, message);
        }

        public void DeletingWithoutReferencedObjectNotAllowed(Row extendedRow, Row rowExtension, string message)
        {
            if ((extendedRow.Status & RowStatus.Deleting) == 0 && !rowExtension.Session.InImport)
            {
                throw new RowException(rowExtension, message);
            }
        }
    }
}