using MailHTML.Dominio.Modelos;
using MailHTML.Dominio.Resultados;

namespace MailHTML.Negocio.Interfaces
{
    public interface IValidadorCotizacion
    {
        Task<ResultadoValidacion> ValidarAsync(CotizacionRenderModel cotizacion);
    }
}