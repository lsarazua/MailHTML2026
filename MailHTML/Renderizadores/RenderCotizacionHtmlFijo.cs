using MailHTML.Dominio.Modelos;
using System.Text;
using System.Threading.Tasks;

namespace MailHTML.Renderizadores
{
    public class RenderCotizacionHtmlFijo : ICotizacionRenderizador
    {
        public Task<ArchivoAdjuntoRenderModel> GenerarArchivoAsync(CotizacionRenderModel modelo, string cotizacionId)
        {
            var html = new SupportEmailTemplateRendererFijo().Render(modelo);

            return Task.FromResult(new ArchivoAdjuntoRenderModel
            {
                NombreArchivo = $"Cotizacion_{cotizacionId}.html",
                ContentType = "text/html",
                Contenido = Encoding.UTF8.GetBytes(html)
            });
        }
    }
}