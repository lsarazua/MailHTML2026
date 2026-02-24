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

        public decimal PrecioConDescuento
        {
            get
            {
                var factor = 1m - (DescuentoPorcentaje / 100m);
                return Math.Round(PrecioUnitario * factor, 2, MidpointRounding.AwayFromZero);
            }
        }

        public decimal SubtotalLinea
            => Math.Round(Cantidad * PrecioConDescuento, 2, MidpointRounding.AwayFromZero);
    }
}
