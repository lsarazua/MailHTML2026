
using MailHTML.Dominio.Modelos;
using System.Text;
using System.Threading.Tasks;

namespace MailHTML.Renderizadores
{
    public class RenderCotizacionHtmlFijoAdapter
    {
        public Task<ArchivoAdjuntoRenderModel> GenerarArchivoAsync(CotizacionRenderModel modelo, string cotizacionId)
        {
            var html = new SupportEmailTemplateRendererFijo().Render(modelo);

            var archivo = new ArchivoAdjuntoRenderModel
            {
                NombreArchivo = $"Cotizacion_{cotizacionId}.html",
                ContentType = "text/html",
                Contenido = Encoding.UTF8.GetBytes(html ?? "")
            };

            return Task.FromResult(archivo);
        }
    }
}