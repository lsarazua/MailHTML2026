using MailHTML.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Datos.Interfaces
{
    public interface ICotizacionRepositorio
    {
        Task<CotizacionEncabezadoModel> ObtenerEncabezadoAsync(string cotizacionId);
        Task<List<CotizacionPartidaModel>> ObtenerPartidasAsync(string cotizacionId);
    }
}
