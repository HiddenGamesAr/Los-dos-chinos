using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Los_dos_chinos
{
    public class Sale
    {
        public string SaleID { get; set; }
        public float Amount { get; set; }
        public DateTime Date { get; set; }
        public int UserID { get; set; }

        public Sale(string ventaID, float monto, DateTime fecha, int usuarioID)
        {
            SaleID = ventaID;
            Amount = monto;
            Date = fecha;
            UserID = usuarioID;
        }
    }
}
