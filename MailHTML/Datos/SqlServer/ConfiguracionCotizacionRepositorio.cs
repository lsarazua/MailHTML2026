using MailHTML.Database;
using MailHTML.Datos.Interfaces;
using MailHTML.Dominio.Modelos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MailHTML.Datos.SqlServer
{
    public class ConfiguracionCotizacionRepositorio : IConfiguracionCotizacionRepositorio
    {
        private readonly ASFAContext _ctx;

        public ConfiguracionCotizacionRepositorio(ASFAContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public Task<CotizacionConfiguracionModel> ObtenerAsync(int territorioId)
        {
            // Aquí debes sustituir por tu consulta real según territorioId.
            // Por ahora devuelve datos de ejemplo con el NUEVO modelo (Terminos/Pago/Footer).

            var cfg = new CotizacionConfiguracionModel
            {
                RenglonesPorHoja = 5,

                UrlLogoPrincipal = "https://www.kangaroocrm.net/KangarooCrm/Logo_AllianceSFA.png",
                UrlLogoFooter = "https://www.kangaroocrm.net/kangaroocrm/LOGO_KANGAROO_CRM.PNG",

                Terminos = new CotizacionTerminosModel
                {
                    Lineas =
                    {
                        "La presente cotización tiene vigencia de 15 días.",
                        "Precios sujetos a cambio sin previo aviso."
                    }
                },

                Pago = new CotizacionPagoModel
                {
                    Texto =
                        "BANCO: BANCO MERCANTIL DEL NORTE SA (BANORTE)\n" +
                        "CLABE: 072910008923603963\n" +
                        "Enviar comprobante a: correodeenvio@kangaroo.mx"
                },

                Footer = new CotizacionFooterModel
                {
                    Empresa = "Farmacia Telefónica Peninsular",
                    Linea1 = "Calle 7 No. 532 x 22 y 24 Local 1",
                    Linea2 = "Col. Maya CP 97134, Mérida Yuc.",
                    Linea3 = "",
                    Correo = "contacto@ftppeninsular.com",
                    Telefono = "(999) 196 0407"
                }
            };

            return Task.FromResult(cfg);
        }

        public async Task<LayoutMailModel?> ObtenerLayoutAsync(string layoutId)
        {
            if (string.IsNullOrWhiteSpace(layoutId))
                throw new ArgumentException("layoutId es requerido.", nameof(layoutId));

            return await _ctx.LayoutsMailWeb
                .FromSqlRaw(@"
                    SELECT TOP 1 Layoutsmail, ID, Nombre, Titulo, Asunto, Texto, Body, Piepagina
                    FROM LayoutsmailWeb
                    WHERE ID = {0}", layoutId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        // Si en algún punto te llega el texto con saltos y quieres convertirlo a líneas:
        private static string[] SplitLineas(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return Array.Empty<string>();

            return texto
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(x => x?.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
        }
    }
}