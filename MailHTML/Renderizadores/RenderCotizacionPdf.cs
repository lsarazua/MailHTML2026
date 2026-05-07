using MailHTML.Dominio.Modelos;
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace MailHTML.Renderizadores
{
    public class RenderCotizacionPdf : ICotizacionRenderizador
    {
        public async Task<ArchivoAdjuntoRenderModel> GenerarArchivoAsync(CotizacionRenderModel modelo, string cotizacionId)
        {
            var html = new SupportEmailTemplateRendererFijo().Render(modelo);
            var pdfBytes = await ConvertirHtmlAPdfAsync(html);

            return new ArchivoAdjuntoRenderModel
            {
                NombreArchivo = $"Cotizacion_{cotizacionId}.pdf",
                ContentType = "application/pdf",
                Contenido = pdfBytes
            };
        }
         

        private static async Task<byte[]> ConvertirHtmlAPdfAsync(string html)
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync();
            await page.SetContentAsync(html);

            return await page.PdfAsync(new PagePdfOptions
            {
                Format = "Letter",
                PrintBackground = true
            });
        }
    }
}