using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Los_dos_chinos
{
    public class Supermarket
    {
        public string CUIT { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }

        public Supermarket(string cUIT, string nombre, string direccion)
        {
            CUIT = cUIT;
            Nombre = nombre;
            Direccion = direccion;
        }
    }
}
