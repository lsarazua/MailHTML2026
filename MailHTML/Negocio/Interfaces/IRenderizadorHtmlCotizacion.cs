using MailHTML.Dominio.Modelos;

namespace MailHTML.Negocio.Interfaces
{
    public interface IRenderizadorHtmlCotizacion
    {
        string Renderizar(CotizacionRenderModel cotizacion);
    }
}