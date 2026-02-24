using MailHTML.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MailHTML.Renderizadores
{
    public class SupportEmailTemplateRendererFijo
    {
        private static readonly CultureInfo CultureMx = new CultureInfo("es-MX");

        private const decimal RowHeightIn = 0.32m;

        private const decimal TopIn = 3.00m;
        private const decimal TableIn = 3.50m;
        private const decimal BottomIn = 3.50m;

        private const int TotRowsCount = 2;
        private const decimal TotRowHeightIn = 0.25m;

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

                htmlPages.Append(BuildSinglePageHtml(
                    encabezado,
                    config,
                    tbodyHtml,
                    isLast,
                    TopIn,
                    TableIn,
                    BottomIn,
                    pageIndex + 1,
                    totalPages
                ));
            }

            return BuildDocument(htmlPages.ToString());
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

        private static string BuildDocument(string pagesHtml)
        {
            string rowH = RowHeightIn.ToString(CultureInfo.InvariantCulture);
            string totH = TotRowHeightIn.ToString(CultureInfo.InvariantCulture);

            return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<title>Cotización</title>

<style>
    @page {{
        size: Letter;
        margin: 0;
    }}

    html, body {{
        margin: 0;
        padding: 0;
        font-family: 'Montserrat', Arial, Helvetica, sans-serif;
        color: #1C1E21;
        -webkit-print-color-adjust: exact;
        print-color-adjust: exact;
    }}

    body {{
        background: #f2f2f2;
        padding: 18px 0;
    }}

    .page {{
        width: 8.5in;
        height: 11in;
        margin: 0 auto 18px auto;
        background: #fff;
        box-sizing: border-box;
        padding: 0.5in;
        box-shadow: 0 8px 28px rgba(0,0,0,.12);
        page-break-after: always;
    }}

    .page:last-child {{
        page-break-after: auto;
        margin-bottom: 0;
    }}

    @media print {{
        body {{
            background: #fff;
            padding: 0;
        }}
        .page {{
            margin: 0;
            box-shadow: none;
        }}
    }}

    .page-grid {{
        width: 7.5in;
        height: 10in;
        display: grid;
        grid-template-rows: var(--top) var(--table) var(--bottom);
        box-sizing: border-box;
    }}

    .top-block, .table-block, .bottom-block {{
        overflow: hidden;
    }}

    .bottom-block {{
        display: grid;
        grid-template-rows: 1fr auto;
    }}

    .header {{
        text-align: center;
        margin: 0;
        padding: 0;
    }}

    .header img {{
        width: 210px;
        height: auto;
        display: inline-block;
        margin-top: 2px;
    }}

    .doc-title {{
        margin-top: 6px;
        font-size: 22px;
        font-weight: 700;
        letter-spacing: 1px;
        color: #1C1E21;
    }}

    .top-info {{
        display: grid;
        grid-template-columns: 1fr 240px;
        gap: 14px;
        align-items: start;
        margin-top: 10px;
    }}

    .info {{
        font-size: 14px;
        color: #4A4D4F;
        line-height: 1.45;
    }}

    .info .label {{
        font-weight: 700;
        color: #1C1E21;
        margin-bottom: 6px;
        display: inline-block;
    }}

    .info p {{
        margin: 2px 0;
    }}

    .quote-meta {{
        text-align: right;
        font-size: 14px;
        color: #4A4D4F;
        line-height: 1.6;
        white-space: nowrap;
        margin-top: 12px;
    }}

    .page-no {{
        font-size: 12px;
        color: #6B6F72;
        margin-top: 2px;
    }}

    .mensaje {{
        font-size: 14px;
        color: #4A4D4F;
        margin: 10px 0 10px 0;
        line-height: 1.40;
    }}

    table {{
        width: 100%;
        border-collapse: collapse;
        table-layout: fixed;
        font-size: 12px;
        color: #1C1E21;
        margin: 0;
    }}

    thead th {{
        background: #C8C8C8;
        color: #1C1E21;
        font-weight: 700;
        border: 1px solid #9A9A9A;
        padding: 5px 4px;
        text-align: center;
        white-space: nowrap;
        overflow: hidden;
        font-size: 11px;
        line-height: 1.1;
    }}

    tbody td {{
        border: 1px solid #9A9A9A;
        padding: 6px;
        text-align: center;
        vertical-align: middle;
        overflow: hidden;
        line-height: 1.1;
        height: {rowH}in;
    }}

    tr.row-fixed {{
        height: {rowH}in;
    }}

    td.nowrap {{
        white-space: nowrap;
        text-overflow: ellipsis;
    }}

    td.col-producto {{
        text-align: left;
        white-space: normal;
    }}

    .producto-text {{
        display: block;
        width: 100%;
        overflow: hidden;
        line-height: 1.15;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
    }}

    .tot-spacer td {{
        border: 0 !important;
        padding: 0 !important;
        height: {totH}in !important;
    }}

    td.tot-empty {{
        border: 0 !important;
        background: transparent !important;
    }}

    td.tot-label {{
        background: #fff;
        font-weight: 700;
        text-align: center;
        border: 1px solid #9A9A9A !important;
        white-space: nowrap;
        padding: 5px 6px !important;
        height: {totH}in !important;
    }}

    td.tot-value {{
        background: #fff;
        text-align: right;
        border: 1px solid #9A9A9A !important;
        white-space: nowrap;
        padding: 5px 10px !important;
        height: {totH}in !important;
    }}

    .section {{
        font-size: 14px;
        color: #4A4D4F;
    }}

    .section h3 {{
        margin: 0 0 8px 0;
        font-size: 14px;
        font-weight: 700;
        color: #1C1E21;
    }}

    .section ul {{
        padding-left: 18px;
        margin: 0;
    }}

    .footer {{
        display: grid;
        grid-template-columns: 1fr auto;
        gap: 18px;
        align-items: end;
        font-size: 13px;
        color: #4A4D4F;
    }}

    .footer img {{
        height: 55px;
        width: auto;
        opacity: 0.18;
        margin-bottom: -6px;
    }}
</style>
</head>

<body>
{pagesHtml}
</body>
</html>";
        }

        private string BuildSinglePageHtml(
            CotizacionEncabezadoModel encabezado,
            CotizacionConfiguracionModel config,
            string tbodyHtml,
            bool isLast,
            decimal topIn,
            decimal tableIn,
            decimal bottomIn,
            int pageNo,
            int totalPages)
        {
            string bottomHtml = BuildBottomBlockHtml(isLast, config);

            string urlLogoPrincipal = string.IsNullOrWhiteSpace(config.UrlLogoPrincipal)
                ? ""
                : config.UrlLogoPrincipal;

            return $@"
<div class=""page"">
  <div class=""page-grid"" style=""--top:{topIn.ToString(CultureInfo.InvariantCulture)}in; --table:{tableIn.ToString(CultureInfo.InvariantCulture)}in; --bottom:{bottomIn.ToString(CultureInfo.InvariantCulture)}in;"">

    <div class=""top-block"">
      <div class=""header"">
        {(string.IsNullOrWhiteSpace(urlLogoPrincipal) ? "" : $@"<img src=""{Html(urlLogoPrincipal)}"" alt=""Logo"" />")}
        <div class=""doc-title"">COTIZACIÓN</div>
      </div>

      <div class=""top-info"">
        <div class=""info"">
          <span class=""label"">Cotización para:</span>
          <p>Nombre: {Html(encabezado.ClienteNombre)}</p>
          <p>Dirección: {Html(encabezado.Direccion)}</p>
          <p>Correo: {Html(encabezado.SolicitanteCorreo)}</p>
          <p>Teléfono: {Html(encabezado.SolicitanteTelefono)}</p>
        </div>

        <div class=""quote-meta"">
          <div>Fecha: {encabezado.Fecha:dd/MM/yyyy}</div>
          <div>N. de cotización: {Html(encabezado.CotizacionId)}</div>
          <div class=""page-no"">Página {pageNo} de {totalPages}</div>
        </div>
      </div>

      <div class=""mensaje"">
        Gracias por solicitar esta cotización. Quedamos atentos a cualquier duda o comentario y será un gusto atenderle si el producto es de su interés.
      </div>
    </div>

    <div class=""table-block"">
      <table>
        <colgroup>
          <col style=""width: 6%;"">
          <col style=""width: 31%;"">
          <col style=""width: 7%;"">
          <col style=""width: 7%;"">
          <col style=""width: 12%;"">
          <col style=""width: 12%;"">
          <col style=""width: 12.5%;"">
          <col style=""width: 12.5%;"">
        </colgroup>

        <thead>
          <tr>
            <th>Num.</th>
            <th>Producto</th>
            <th>Cant.</th>
            <th>Und.</th>
            <th>Precio Unit.</th>
            <th>Descuento(%)</th>
            <th>Precio Dcto.</th>
            <th>Sub total</th>
          </tr>
        </thead>

        <tbody>
          {tbodyHtml}
        </tbody>
      </table>
    </div>

    <div class=""bottom-block"">
      {bottomHtml}
    </div>

  </div>
</div>";
        }

        private string BuildBottomBlockHtml(bool isLast, CotizacionConfiguracionModel config)
        {
            if (!isLast) return "<div></div>";

            var sb = new StringBuilder();

            if (config?.Terminos?.Lineas?.Any() == true)
            {
                sb.Append(@"
<div class=""section"">
  <h3>Término y condiciones:</h3>
  <ul>");
                foreach (var linea in config.Terminos.Lineas.Where(x => !string.IsNullOrWhiteSpace(x)))
                    sb.Append("<li>" + Html(linea) + "</li>");
                sb.Append(@"
  </ul>
</div>");
            }

            if (!string.IsNullOrWhiteSpace(config?.Pago?.Texto))
            {
                sb.Append(@"
<div class=""section"" style=""margin-top:12px;"">
  <h3>Formas de pago:</h3>
  <p>");
                var lineas = config.Pago.Texto.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (var l in lineas)
                {
                    if (string.IsNullOrWhiteSpace(l)) { sb.Append("<br>"); }
                    else { sb.Append(Html(l) + "<br>"); }
                }
                sb.Append(@"
  </p>
</div>");
            }

            string footerHtml = BuildFooterHtml(config);
            if (!string.IsNullOrWhiteSpace(footerHtml))
                sb.Append(footerHtml);

            return sb.ToString();
        }

        private static string BuildFooterHtml(CotizacionConfiguracionModel config)
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
                : $@"<img src=""{Html(config.UrlLogoFooter)}"" alt=""Logo"" />";

            return $@"
<div class=""footer"">
  <div>
    {(string.IsNullOrWhiteSpace(f.Empresa) ? "" : $"<strong>{Html(f.Empresa)}</strong><br>")}
    {(string.IsNullOrWhiteSpace(f.Linea1) ? "" : Html(f.Linea1) + "<br>")}
    {(string.IsNullOrWhiteSpace(f.Linea2) ? "" : Html(f.Linea2) + "<br>")}
    {(string.IsNullOrWhiteSpace(f.Linea3) ? "" : Html(f.Linea3) + "<br>")}
    {(string.IsNullOrWhiteSpace(f.Correo) ? "" : Html(f.Correo) + "<br>")}
    {(string.IsNullOrWhiteSpace(f.Telefono) ? "" : Html(f.Telefono))}
  </div>
  <div>
    {logo}
  </div>
</div>";
        }

        private string BuildTbodyHtml(
            List<CotizacionPartidaModel> pageItems,
            int fixedRows,
            int startIndex,
            bool isLast,
            decimal iva,
            decimal total)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < fixedRows; i++)
            {
                var item = i < pageItems.Count ? pageItems[i] : null;

                if (item == null)
                {
                    sb.Append(@"
<tr class=""row-fixed"">
  <td>&nbsp;</td>
  <td class=""col-producto""><span class=""producto-text"">&nbsp;</span></td>
  <td>&nbsp;</td>
  <td>&nbsp;</td>
  <td class=""nowrap"">&nbsp;</td>
  <td class=""nowrap"">&nbsp;</td>
  <td class=""nowrap"">&nbsp;</td>
  <td class=""nowrap"">&nbsp;</td>
</tr>");
                }
                else
                {
                    int num = item.Numero > 0 ? item.Numero : (startIndex + i + 1);

                    sb.Append($@"
<tr class=""row-fixed"">
  <td class=""nowrap"">{num}</td>
  <td class=""col-producto""><span class=""producto-text"">{Html(item.ProductoNombre)}</span></td>
  <td class=""nowrap"">{item.Cantidad:0.##}</td>
  <td class=""nowrap"">{Html(item.Unidad)}</td>
  <td class=""nowrap"">{Money(item.PrecioUnitario)}</td>
  <td class=""nowrap"">{item.DescuentoPorcentaje.ToString("0.##", CultureMx)}</td>
  <td class=""nowrap"">{Money(item.PrecioConDescuento)}</td>
  <td class=""nowrap"">{Money(item.SubtotalLinea)}</td>
</tr>");
                }
            }

            if (isLast)
            {
                sb.Append($@"
<tr>
  <td class=""tot-empty"" colspan=""6""></td>
  <td class=""tot-label"">IVA</td>
  <td class=""tot-value"">{Money(iva)}</td>
</tr>
<tr>
  <td class=""tot-empty"" colspan=""6""></td>
  <td class=""tot-label"">Total:</td>
  <td class=""tot-value"">{Money(total)}</td>
</tr>");
            }
            else
            {
                for (int i = 0; i < TotRowsCount; i++)
                    sb.Append(@"<tr class=""tot-spacer""><td colspan=""8"">&nbsp;</td></tr>");
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