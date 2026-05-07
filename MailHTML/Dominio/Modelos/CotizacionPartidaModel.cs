using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Dominio.Modelos
{
    public class CotizacionPartidaModel
    {
        public int Numero { get; set; }
        public string ProductoId { get; set; } = "";
        public string ProductoNombre { get; set; } = "";
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; } = "PZA";
        public decimal PrecioUnitario { get; set; }
        public decimal DescuentoPorcentaje { get; set; }
        public int PiezasGratuitas { get; set; }
        public decimal MontoDescuento { get; set; }
        public decimal SubtotalLinea { get; set; }
    }
}
