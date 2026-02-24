using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Dominio.Modelos
{
    public class CotizacionRenderModel
    {
        public CotizacionEncabezadoModel Encabezado { get; set; } = new();
        public List<CotizacionPartidaModel> Partidas { get; set; } = new();
        public CotizacionTotalesModel Totales { get; set; } = new();
        public CotizacionConfiguracionModel Configuracion { get; set; } = new();
    }
}
