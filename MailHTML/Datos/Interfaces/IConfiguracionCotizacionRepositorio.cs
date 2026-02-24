using MailHTML.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Datos.Interfaces
{
    public interface IConfiguracionCotizacionRepositorio
    {
        Task<CotizacionConfiguracionModel> ObtenerAsync(int territorioId);
        Task<LayoutMailModel?> ObtenerLayoutAsync(string layoutId);
    }
}
