using MailHTML.Dominio.Resultados;

namespace MailHTML.Negocio.Interfaces
{
    public interface IServicioImpresionCotizacion
    {
        Task<ResultadoImpresionCotizacion> ImprimirAsync(string cotizacionId);
    }
}