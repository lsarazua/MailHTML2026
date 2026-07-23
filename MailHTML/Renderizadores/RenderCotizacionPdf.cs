using MailHTML.Dominio.Modelos;
using MailHTML.Renderizadores.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MailHTML.Renderizadores
{
    public class RenderCotizacionPdf : ICotizacionRenderizador
    {
        private static bool QuestPdfConfigurado;
        private static readonly object BloqueoConfiguracion = new object();
        private static readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public async Task<ArchivoAdjuntoRenderModel> GenerarArchivoAsync(CotizacionRenderModel modelo, string cotizacionId)
        {
            if (modelo == null) throw new ArgumentNullException(nameof(modelo));

            ConfigurarQuestPdf();

            var logoPrincipal = await DescargarImagenAsync(modelo.Configuracion?.UrlLogoPrincipal);
            var logoFooter = await DescargarImagenAsync(modelo.Configuracion?.UrlLogoFooter);

            var documento = new CotizacionPdfDocument(modelo, logoPrincipal, logoFooter);
            var pdfBytes = documento.GeneratePdf();

            return new ArchivoAdjuntoRenderModel
            {
                NombreArchivo = $"Cotizacion_{cotizacionId}.pdf",
                ContentType = "application/pdf",
                Contenido = pdfBytes
            };
        }

        private static async Task<byte[]> DescargarImagenAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new Exception($"La URL del logo no es válida: {url}");

            try
            {
                using var response = await HttpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                var contentType = response.Content.Headers.ContentType?.MediaType;

                if (string.IsNullOrWhiteSpace(contentType) || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    throw new Exception($"La URL no devolvió una imagen válida. URL={url}, ContentType={contentType}");

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"No fue posible descargar el logo configurado en '{url}'. {ex.Message}", ex);
            }
        }

        private static void ConfigurarQuestPdf()
        {
            if (QuestPdfConfigurado) return;

            lock (BloqueoConfiguracion)
            {
                if (QuestPdfConfigurado) return;

                QuestPDF.Settings.License = LicenseType.Community;
                QuestPDF.Settings.EnableDebugging = true;
                QuestPdfConfigurado = true;
            }
        }
    }
}