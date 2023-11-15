using System;

namespace Los_dos_chinos
{
    public class Article
    {
        public Int64 ArticuloID { get; set; }
        public string Detalle { get; set; }
        public string Presentacion { get; set; }
        public float PrecioCompra { get; set; }
        public float PrecioVenta { get; set; }
        public int Stock {  get; set; }
        public string Proveedor { get; set; }

        public Article (string detalle, string presentacion, float precioCompra, float precioVenta)
        {
            Detalle = detalle;
            Presentacion = presentacion;
            PrecioCompra = precioCompra;
            PrecioVenta = precioVenta;
        }

        public Article (Int64 articuloID, string detalle, string presentacion, float precioCompra, float precioVenta, int stock, string _proveedor)
        {
            ArticuloID = articuloID;
            Detalle = detalle;
            Presentacion = presentacion;
            PrecioCompra = precioCompra;
            PrecioVenta = precioVenta;
            Stock = stock;
            Proveedor = _proveedor;
        }

        public Article (Int64 articuloid,string detalle, string presentacion, float precioVenta)
        {
            ArticuloID = articuloid;
            Detalle = detalle;
            Presentacion = presentacion;
            PrecioVenta = precioVenta;
        }

        public Article (Int64 articuloid, string detalle)
        {
            ArticuloID = articuloid;
            Detalle = detalle;
        }
    }
}
