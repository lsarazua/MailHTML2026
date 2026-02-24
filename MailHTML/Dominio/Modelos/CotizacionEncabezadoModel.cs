using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Dominio.Modelos
{
    public class CotizacionEncabezadoModel
    {
        public string CotizacionId { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;

        public string ClienteId { get; set; } = "";
        public string ClienteNombre { get; set; } = "";

        public string Direccion { get; set; } = "";

        public int TerritorioId { get; set; }

        public string SolicitanteNombre { get; set; } = "";
        public int SolicitanteEmpleadoId { get; set; }

        public string SolicitanteCorreo { get; set; } = "";
        public string SolicitanteTelefono { get; set; } = "";
    }
}
