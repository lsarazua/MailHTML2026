using MailHTML.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MailHTML.Renderizadores
{
    public sealed class SupportEmailTemplateRendererFijoEmail
    {
        private static readonly CultureInfo CultureMx = new CultureInfo("es-MX");

        private const decimal RowHeightIn = 0.32m;

        private const decimal TopIn = 3.00m;
        private const decimal TableIn = 3.50m;
        private const decimal BottomIn = 3.50m;

        private const int TotRowsCount = 2;
        private const decimal TotRowHeightIn = 0.25m;

        private const int Dpi = 96;

        private static int InToPx(decimal inches)
            => (int)Math.Round((double)(inches * Dpi), MidpointRounding.AwayFromZero);

        public string Render(CotizacionRenderModel modelo)
        {
            if (modelo == null) throw new ArgumentNullException(nameof(modelo));

            var encabezado = modelo.Encabezado ?? new CotizacionEncabezadoModel();
            var partidas = modelo.Partidas ?? new List<CotizacionPartidaModel>();
            var totales = modelo.Totales ?? new CotizacionTotalesModel();
            var config = modelo.Configuracion ?? new CotizacionConfiguracionModel();

            int rowsPerPage = config.RenglonesPorHoja > 0 ? config.RenglonesPorHoja : 5;
            var pages = BuildPages(partidas, rowsPerPage);

            int totalPages = pages.Count;
            var htmlPages = new StringBuilder();

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                var page = pages[pageIndex];
                bool isLast = pageIndex == totalPages - 1;

                string tbodyHtml = BuildTbodyHtml(
                    page.Items,
                    rowsPerPage,
                    page.StartIndex,
                    isLast,
                    totales.Iva,
                    totales.Total
                );

                htmlPages.Append(BuildSinglePageHtml_EmailSafe(
                    encabezado,
                    config,
                    tbodyHtml,
                    isLast,
                    pageIndex + 1,
                    totalPages
                ));
            }

            return BuildDocument_EmailSafe(htmlPages.ToString());
        }

        private sealed class PageDef
        {
            public int StartIndex { get; init; }
            public List<CotizacionPartidaModel> Items { get; init; } = new List<CotizacionPartidaModel>();
        }

        private static List<PageDef> BuildPages(IReadOnlyList<CotizacionPartidaModel> partidas, int rowsPerPage)
        {
            var result = new List<PageDef>();
            int n = partidas?.Count ?? 0;

            if (n == 0)
            {
                result.Add(new PageDef { StartIndex = 0, Items = new List<CotizacionPartidaModel>() });
                return result;
            }

            for (int i = 0; i < n; i += rowsPerPage)
                result.Add(new PageDef { StartIndex = i, Items = partidas.Skip(i).Take(rowsPerPage).ToList() });

            return result;
        }

        private string BuildDocument_EmailSafe(string pagesHtml)
        {
            return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<title>Cotización</title>
<style>
  body {{
    margin:0;
    padding:0;
    background:#f2f2f2;
    font-family: Arial, Helvetica, sans-serif;
    color:#1C1E21;
  }}
  table {{
    border-collapse: collapse;
    border-spacing: 0;
  }}
  img {{
    border:0;
    outline:none;
    text-decoration:none;
    display:block;
  }}
  @media print {{
    body {{ background:#ffffff !important; }}
    .kc-page {{ box-shadow:none !important; margin:0 !important; }}
  }}
</style>
</head>
<body>
<table role=""presentation"" width=""100%"" style=""background:#f2f2f2; padding:18px 0; margin:0;"">
  <tr>
    <td align=""center"">
      {pagesHtml}
    </td>
  </tr>
</table>
</body>
</html>";
        }

        private string BuildSinglePageHtml_EmailSafe(
            CotizacionEncabezadoModel encabezado,
            CotizacionConfiguracionModel config,
            string tbodyHtml,
            bool isLast,
            int pageNo,
            int totalPages)
        {
            int pageW = InToPx(8.5m);
            int pageH = InToPx(11m);
            int padding = InToPx(0.5m);

            int innerW = pageW - (padding * 2);

            int topH = InToPx(TopIn);
            int tableH = InToPx(TableIn);
            int bottomH = InToPx(BottomIn);

            string urlLogoPrincipal = string.IsNullOrWhiteSpace(config.UrlLogoPrincipal) ? "" : config.UrlLogoPrincipal;

            string topBlock = BuildTopBlock_EmailSafe(encabezado, urlLogoPrincipal, pageNo, totalPages, innerW);
            string tableBlock = BuildTableBlock_EmailSafe(tbodyHtml, isLast);
            string bottomBlock = BuildBottomBlock_EmailSafe(isLast, config, innerW);

            string pageBreak = isLast ? "page-break-after:auto;" : "page-break-after:always;";

            return $@"
<table class=""kc-page"" role=""presentation"" width=""{pageW}"" style=""width:{pageW}px; height:{pageH}px; background:#ffffff; margin:0 auto 18px auto; box-shadow:0 8px 28px rgba(0,0,0,.12); {pageBreak}"">
  <tr>
    <td style=""padding:{padding}px; vertical-align:top;"">
      <table role=""presentation"" width=""100%"" style=""width:{innerW}px;"">
        <tr>
          <td style=""height:{topH}px; vertical-align:top; overflow:hidden;"">
            {topBlock}
          </td>
        </tr>

        <tr>
          <td style=""height:{tableH}px; vertical-align:top; overflow:hidden;"">
            {tableBlock}
          </td>
        </tr>

        <tr>
          <td style=""height:{bottomH}px; vertical-align:top; overflow:hidden;"">
            {bottomBlock}
          </td>
        </tr>
      </table>
    </td>
  </tr>
</table>";
        }

        private string BuildTopBlock_EmailSafe(
            CotizacionEncabezadoModel encabezado,
            string urlLogoPrincipal,
            int pageNo,
            int totalPages,
            int innerW)
        {
            string logoHtml = string.IsNullOrWhiteSpace(urlLogoPrincipal)
                ? ""
                : $@"<tr><td align=""center"" style=""padding:0;"">
                      <img src=""{Html(urlLogoPrincipal)}"" alt=""Logo"" style=""width:210px; height:auto; display:inline-block; margin-top:2px;"" />
                    </td></tr>";

            return $@"
<table role=""presentation"" width=""100%"" style=""width:{innerW}px;"">
  {logoHtml}

  <tr>
    <td align=""center"" style=""padding:6px 0 0 0; font-size:22px; font-weight:700; letter-spacing:1px; color:#1C1E21;"">
      COTIZACIÓN
    </td>
  </tr>

  <tr>
    <td style=""padding:10px 0 0 0;"">
      <table role=""presentation"" width=""100%"" style=""width:{innerW}px;"">
        <tr>
          <td style=""width:{innerW - 240}px; vertical-align:top; font-size:14px; color:#4A4D4F; line-height:1.45;"">
            <div style=""font-weight:700; color:#1C1E21; margin-bottom:6px;"">Cotización para:</div>
            <div style=""margin:2px 0;"">Nombre: {Html(encabezado.ClienteNombre)}</div>
            <div style=""margin:2px 0;"">Dirección: {Html(encabezado.Direccion)}</div>
            <div style=""margin:2px 0;"">Correo: {Html(encabezado.SolicitanteCorreo)}</div>
            <div style=""margin:2px 0;"">Teléfono: {Html(encabezado.SolicitanteTelefono)}</div>
          </td>

          <td style=""width:240px; vertical-align:top; text-align:right; font-size:14px; color:#4A4D4F; line-height:1.6; white-space:nowrap; padding-top:2px;"">
            <div>Fecha: {encabezado.Fecha:dd/MM/yyyy}</div>
            <div>N. de cotización: {Html(encabezado.CotizacionId)}</div>
            <div style=""font-size:12px; color:#6B6F72; margin-top:2px;"">Página {pageNo} de {totalPages}</div>
          </td>
        </tr>
      </table>
    </td>
  </tr>

  <tr>
    <td style=""padding:10px 0 10px 0; font-size:14px; color:#4A4D4F; line-height:1.40;"">
      Gracias por solicitar esta cotización. Quedamos atentos a cualquier duda o comentario y será un gusto atenderle si el producto es de su interés.
    </td>
  </tr>
</table>";
        }

        private string BuildTableBlock_EmailSafe(string tbodyHtml, bool isLast)
        {
            var sb = new StringBuilder();

            sb.Append($@"
<table role=""presentation"" width=""100%"" style=""width:100%; table-layout:fixed; font-size:12px; color:#1C1E21; margin:0; border-collapse:collapse;"">
  <tr>
    <td style=""background:#C8C8C8; border:1px solid #9A9A9A; padding:5px 4px; text-align:center; font-size:11px; font-weight:700; width:6%; white-space:nowrap;"">Num.</td>
    <td style=""background:#C8C8C8; border:1px solid #9A9A9A; padding:5px 4px; text-align:center; font-size:11px; font-weight:700; width:31%; white-space:nowrap;"">Producto</td>
    <td style=""background:#C8C8C8; border:1px solid #9A9A9A; padding:5px 4px; text-align:center; font-size:11px; font-weight:700; width:7%; white-space:nowrap;"">Cant.</td>
    <td style=""background:#C8C8C8; border:1px solid #9A9A9A; padding:5px 4px; text-align:center; font-size:11px; font-weight:700; width:7%; white-space:nowrap;"">Und.</td>
    <td style=""background:#C8C8C8; border:1px solid #9A9A9A; padding:5px 4px; text-align:center; font-size:11px; font-weight:700; width:12%; white-space:nowrap;"">Precio Unit.</td>
    <td style=""background:#C8C8C8; border:1px solid #9A9A9A; padding:5px 4px; text-align:center; font-size:11px; font-weight:700; width:12%; white-space:nowrap;"">Descuento(%)</td>
    <td style=""background:#C8C8C8; border:1px solid #9A9A9A; padding:5px 4px; text-align:center; font-size:11px; font-weight:700; width:12.5%; white-space:nowrap;"">Precio Dcto.</td>
    <td style=""background:#C8C8C8; border:1px solid #9A9A9A; padding:5px 4px; text-align:center; font-size:11px; font-weight:700; width:12.5%; white-space:nowrap;"">Sub total</td>
  </tr>

  {tbodyHtml}
</table>");

            // NUEVO: texto en páginas intermedias (sin totales)
            if (!isLast)
            {
                sb.Append(@"
<table role=""presentation"" width=""100%"" style=""width:100%; margin-top:8px;"">
  <tr>
    <td style=""font-size:12px; color:#6B6F72; font-weight:700; text-align:right;"">
      Totales al final. Continuar con la siguiente hoja.
    </td>
  </tr>
</table>");
            }

            return sb.ToString();
        }

        private string BuildBottomBlock_EmailSafe(bool isLast, CotizacionConfiguracionModel config, int innerW)
        {
            var sb = new StringBuilder();

            // NUEVO: footer SIEMPRE (todas las hojas)
            string footerAlways = BuildFooterHtml_EmailSafe(config, innerW);
            if (!string.IsNullOrWhiteSpace(footerAlways))
                sb.Append(footerAlways);

            // SOLO ÚLTIMA HOJA: términos + pago (como antes)
            if (isLast)
            {
                if (config?.Terminos?.Lineas?.Any() == true)
                {
                    sb.Append($@"
<table role=""presentation"" width=""100%"" style=""width:{innerW}px; margin-top:14px;"">
  <tr>
    <td style=""font-size:14px; color:#4A4D4F;"">
      <div style=""font-size:14px; font-weight:700; color:#1C1E21; margin:0 0 8px 0;"">Término y condiciones:</div>
      <ul style=""padding-left:18px; margin:0;"">");

                    foreach (var linea in config.Terminos.Lineas.Where(x => !string.IsNullOrWhiteSpace(x)))
                        sb.Append("<li style=\"margin:0; padding:0;\">" + Html(linea) + "</li>");

                    sb.Append(@"
      </ul>
    </td>
  </tr>
</table>");
                }

                if (!string.IsNullOrWhiteSpace(config?.Pago?.Texto))
                {
                    sb.Append($@"
<table role=""presentation"" width=""100%"" style=""width:{innerW}px; margin-top:12px;"">
  <tr>
    <td style=""font-size:14px; color:#4A4D4F;"">
      <div style=""font-size:14px; font-weight:700; color:#1C1E21; margin:0 0 8px 0;"">Formas de pago:</div>
      <div style=""font-size:14px; color:#4A4D4F; line-height:1.45;"">");

                    var lineas = config.Pago.Texto.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (var l in lineas)
                    {
                        if (string.IsNullOrWhiteSpace(l)) sb.Append("<br>");
                        else sb.Append(Html(l) + "<br>");
                    }

                    sb.Append(@"
      </div>
    </td>
  </tr>
</table>");
                }
            }

            return sb.ToString();
        }

        private static string BuildFooterHtml_EmailSafe(CotizacionConfiguracionModel config, int innerW)
        {
            if (config == null) return "";

            if (!string.IsNullOrWhiteSpace(config.Footer?.FooterHtml))
                return config.Footer.FooterHtml;

            var f = config.Footer ?? new CotizacionFooterModel();

            bool hasAny =
                !string.IsNullOrWhiteSpace(f.Empresa) ||
                !string.IsNullOrWhiteSpace(f.Linea1) ||
                !string.IsNullOrWhiteSpace(f.Linea2) ||
                !string.IsNullOrWhiteSpace(f.Linea3) ||
                !string.IsNullOrWhiteSpace(f.Correo) ||
                !string.IsNullOrWhiteSpace(f.Telefono) ||
                !string.IsNullOrWhiteSpace(config.UrlLogoFooter);

            if (!hasAny) return "";

            string logo = string.IsNullOrWhiteSpace(config.UrlLogoFooter)
                ? ""
                : $@"<img src=""{Html(config.UrlLogoFooter)}"" alt=""Logo"" style=""height:55px; width:auto; opacity:0.18;"" />";

            string empresa = string.IsNullOrWhiteSpace(f.Empresa) ? "" : $"<strong>{Html(f.Empresa)}</strong><br>";
            string l1 = string.IsNullOrWhiteSpace(f.Linea1) ? "" : Html(f.Linea1) + "<br>";
            string l2 = string.IsNullOrWhiteSpace(f.Linea2) ? "" : Html(f.Linea2) + "<br>";
            string l3 = string.IsNullOrWhiteSpace(f.Linea3) ? "" : Html(f.Linea3) + "<br>";
            string mail = string.IsNullOrWhiteSpace(f.Correo) ? "" : Html(f.Correo) + "<br>";
            string tel = string.IsNullOrWhiteSpace(f.Telefono) ? "" : Html(f.Telefono);

            return $@"
<table role=""presentation"" width=""100%"" style=""width:{innerW}px; margin-top:14px;"">
  <tr>
    <td style=""vertical-align:bottom; font-size:13px; color:#4A4D4F; line-height:1.35;"">
      {empresa}{l1}{l2}{l3}{mail}{tel}
    </td>
    <td align=""right"" style=""vertical-align:bottom;"">
      {logo}
    </td>
  </tr>
</table>";
        }

        private string BuildTbodyHtml(
            List<CotizacionPartidaModel> pageItems,
            int fixedRows,
            int startIndex,
            bool isLast,
            decimal iva,
            decimal total)
        {
            int rowH = InToPx(RowHeightIn);
            int totH = InToPx(TotRowHeightIn);

            var sb = new StringBuilder();

            for (int i = 0; i < fixedRows; i++)
            {
                var item = i < pageItems.Count ? pageItems[i] : null;

                if (item == null)
                {
                    sb.Append($@"
<tr style=""height:{rowH}px;"">
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle;"">&nbsp;</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:left; vertical-align:middle;"">&nbsp;</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle;"">&nbsp;</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle;"">&nbsp;</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">&nbsp;</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">&nbsp;</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">&nbsp;</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">&nbsp;</td>
</tr>");
                }
                else
                {
                    int num = item.Numero > 0 ? item.Numero : (startIndex + i + 1);

                    sb.Append($@"
<tr style=""height:{rowH}px;"">
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">{num}</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:left; vertical-align:middle;"">{Html(item.ProductoNombre)}</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">{item.Cantidad:0.##}</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">{Html(item.Unidad)}</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">{Money(item.PrecioUnitario)}</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">{item.DescuentoPorcentaje.ToString("0.##", CultureMx)}</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">{Money(item.MontoDescuento)}</td>
  <td style=""border:1px solid #9A9A9A; padding:6px; text-align:center; vertical-align:middle; white-space:nowrap;"">{Money(item.SubtotalLinea)}</td>
</tr>");
                }
            }

            if (isLast)
            {
                sb.Append($@"
<tr>
  <td colspan=""6"" style=""border:0; padding:0;""></td>
  <td style=""background:#fff; font-weight:700; text-align:center; border:1px solid #9A9A9A; white-space:nowrap; padding:5px 6px; height:{totH}px;"">IVA</td>
  <td style=""background:#fff; text-align:right; border:1px solid #9A9A9A; white-space:nowrap; padding:5px 10px; height:{totH}px;"">{Money(iva)}</td>
</tr>
<tr>
  <td colspan=""6"" style=""border:0; padding:0;""></td>
  <td style=""background:#fff; font-weight:700; text-align:center; border:1px solid #9A9A9A; white-space:nowrap; padding:5px 6px; height:{totH}px;"">Total:</td>
  <td style=""background:#fff; text-align:right; border:1px solid #9A9A9A; white-space:nowrap; padding:5px 10px; height:{totH}px;"">{Money(total)}</td>
</tr>");
            }
            else
            {
                for (int i = 0; i < TotRowsCount; i++)
                    sb.Append($@"<tr><td colspan=""8"" style=""border:0; padding:0; height:{totH}px;"">&nbsp;</td></tr>");
            }

            return sb.ToString();
        }

        private static string Money(decimal value)
            => value.ToString("C2", CultureMx);

        private static string Html(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }
    }
}