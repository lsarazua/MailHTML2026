using MailHTML.Dominio.Modelos;
using System.Threading.Tasks;

namespace MailHTML.Renderizadores
{
    public interface ICotizacionRenderizador
    {
        Task<ArchivoAdjuntoRenderModel> GenerarArchivoAsync(CotizacionRenderModel modelo, string cotizacionId);
        //Task<ArchivoRenderizadoModel> GenerarArchivoAsync(CotizacionRenderModel modelo, string cotizacionId);
    }
}