using MailHTML.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MailHTML.Renderizadores
{
    public class SupportEmailTemplateRenderer
    {
        private const decimal IvaRate = 0.16m;
        private static readonly CultureInfo CultureMx = new CultureInfo("es-MX");

        private const decimal RowHeightIn = 0.32m;

        private const int NormalRows = 5;     // <= 10 -> páginas de 5
        private const int BigRows = 9;        // > 10 -> páginas intermedias de 9
        private const int BigThreshold = 11;

        private const decimal TopIn_Normal = 3.00m;
        private const decimal TableIn_Normal = 3.50m;
        private const decimal BottomIn_Normal = 3.50m;

        private const decimal TopIn_Inter = 3.00m;
        private const decimal TableIn_Inter = 4.80m;
        private const decimal BottomIn_Inter = 2.20m;

        private const int TotRowsCount = 2;
        private const decimal TotRowHeightIn = 0.25m;

        public string Render(SupportEmailModel model, int productRows)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (productRows < 1) productRows = 1;

            var products = BuildFakeProducts(productRows);
            return Render(model, products);
        }

        public string Render(SupportEmailModel model, IReadOnlyList<QuoteLine> products)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            products ??= Array.Empty<QuoteLine>();

            var pages = BuildPages(products);

            decimal subTotal = products.Sum(x => x.LineTotal);
            decimal iva = RoundMoney(subTotal * IvaRate);
            decimal total = RoundMoney(subTotal + iva);

            int totalPages = pages.Count;
            var htmlPages = new StringBuilder();

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                var page = pages[pageIndex];
                bool isLast = pageIndex == totalPages - 1;

                var layout = GetLayout(products.Count, isLast);

                string tbodyHtml = BuildTbodyHtml(
                    page.Items,
                    page.FixedRows,
                    page.StartIndex,
                    isLast,
                    iva,
                    total
                );

                htmlPages.Append(BuildSinglePageHtml(
                    model,
                    tbodyHtml,
                    isLast,
                    layout.top,
                    layout.table,
                    layout.bottom,
                    pageIndex + 1,
                    totalPages
                ));
            }

            return BuildDocument(htmlPages.ToString());
        }

        private static (decimal top, decimal table, decimal bottom) GetLayout(int totalProducts, bool isLast)
        {
            if (totalProducts < BigThreshold) return (TopIn_Normal, TableIn_Normal, BottomIn_Normal);
            if (isLast) return (TopIn_Normal, TableIn_Normal, BottomIn_Normal);
            return (TopIn_Inter, TableIn_Inter, BottomIn_Inter);
        }

        private sealed class PageDef
        {
            public int StartIndex { get; init; }
            public int FixedRows { get; init; }
            public List<QuoteLine> Items { get; init; } = new();
        }

        private static List<PageDef> BuildPages(IReadOnlyList<QuoteLine> products)
        {
            var result = new List<PageDef>();

            int n = products?.Count ?? 0;

            if (n == 0)
            {
                result.Add(new PageDef { StartIndex = 0, FixedRows = NormalRows, Items = new List<QuoteLine>() });
                return result;
            }

            // <= 10 => páginas de 5
            if (n < BigThreshold)
            {
                for (int i = 0; i < n; i += NormalRows)
                {
                    result.Add(new PageDef
                    {
                        StartIndex = i,
                        FixedRows = NormalRows,
                        Items = products.Skip(i).Take(NormalRows).ToList()
                    });
                }
                return result;
            }

            // > 10 => secuencia:
            // - mientras queden >= 11 => página de 9
            // - si quedan 6..10 => página de 5
            // - última hoja (siempre) => 1..5 productos en grid de 5
            int consumed = 0;
            int remaining = n;

            while (remaining > NormalRows)
            {
                int take = remaining >= 11 ? BigRows : NormalRows; // >=11 -> 9, 6..10 -> 5

                result.Add(new PageDef
                {
                    StartIndex = consumed,
                    FixedRows = take == BigRows ? BigRows : NormalRows,
                    Items = products.Skip(consumed).Take(take).ToList()
                });

                consumed += take;
                remaining = n - consumed;
            }

            // Última hoja (1..5)
            result.Add(new PageDef
            {
                StartIndex = consumed,
                FixedRows = NormalRows,
                Items = products.Skip(consumed).Take(remaining).ToList()
            });

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
            SupportEmailModel model,
            string tbodyHtml,
            bool isLast,
            decimal topIn,
            decimal tableIn,
            decimal bottomIn,
            int pageNo,
            int totalPages)
        {
            string bottomHtml = BuildBottomBlockHtml(isLast);

            return $@"
<div class=""page"">
  <div class=""page-grid"" style=""--top:{topIn.ToString(CultureInfo.InvariantCulture)}in; --table:{tableIn.ToString(CultureInfo.InvariantCulture)}in; --bottom:{bottomIn.ToString(CultureInfo.InvariantCulture)}in;"">

    <div class=""top-block"">
      <div class=""header"">
        <img src=""https://www.kangaroocrm.net/KangarooCrm/Logo_AllianceSFA.png"" alt=""Alliance"" />
        <div class=""doc-title"">COTIZACIÓN</div>
      </div>

      <div class=""top-info"">
        <div class=""info"">
          <span class=""label"">Cotización para:</span>
          <p>Nombre: {Html(model.CustomerName)}</p>
          <p>Dirección: {Html(model.AddressId)}</p>
          <p>Correo: {Html(model.RequesterName)}</p>
          <p>Teléfono: {Html(model.RequesterEmployeeId.ToString())}</p>
        </div>

        <div class=""quote-meta"">
          <div>Fecha: {DateTime.Now:dd/MM/yyyy}</div>
          <div>N. de cotización: {Html(model.CustomerId)}</div>
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

        private string BuildBottomBlockHtml(bool isLast)
        {
            string terms = isLast
                ? @"
<div class=""section"">
  <h3>Término y condiciones:</h3>
  <ul>
    <li>La presente cotización tiene una vigencia de 15 días naturales.</li>
    <li>Los precios están sujetos a cambios sin previo aviso.</li>
    <li>El pedido se confirmará al recibir el pago o anticipo.</li>
  </ul>
</div>

<div class=""section"" style=""margin-top:12px;"">
  <h3>Formas de pago:</h3>
  <p>
    <strong>BANCO</strong><br>
    #000000000000<br>
    Envíe el comprobante a: correodeenvio@kangaroo.mx
  </p>
</div>"
                : @"<div></div>";

            string footer = @"
<div class=""footer"">
  <div>
    <strong>Farmacia Telefónica Peninsular</strong><br>
    Calle 7 No. 532 x 22 y 24 Local 1<br>
    Col. Maya CP 97134, Mérida Yuc.<br>
    contacto@ftppeninsular.com<br>
    (999) 196 0407
  </div>
  <div>
    <img src=""https://www.kangaroocrm.net/kangaroocrm/LOGO_KANGAROO_CRM.PNG"" alt=""Kangaroo"" />
  </div>
</div>";

            return $@"{terms}{footer}";
        }

        private string BuildTbodyHtml(
            List<QuoteLine> pageItems,
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
                    int num = startIndex + i + 1;

                    sb.Append($@"
<tr class=""row-fixed"">
  <td class=""nowrap"">{num}</td>
  <td class=""col-producto""><span class=""producto-text"">{Html(item.ProductName)}</span></td>
  <td class=""nowrap"">{item.Quantity:0.##}</td>
  <td class=""nowrap"">{Html(item.Unit)}</td>
  <td class=""nowrap"">{Money(item.UnitPrice)}</td>
  <td class=""nowrap"">{item.DiscountPercent.ToString("0.##", CultureMx)}</td>
  <td class=""nowrap"">{Money(item.UnitPriceAfterDiscount)}</td>
  <td class=""nowrap"">{Money(item.LineTotal)}</td>
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

        private static List<QuoteLine> BuildFakeProducts(int count)
        {
            var list = new List<QuoteLine>(count);

            for (int i = 1; i <= count; i++)
            {
                decimal qty = (i % 3) + 1;
                decimal unitPrice = 125.50m + (i * 3.10m);
                decimal discount = (i % 4) * 5m;

                list.Add(new QuoteLine
                {
                    ProductName = $"Producto #{i} con descripción ejemplo para 2 renglones",
                    Quantity = qty,
                    Unit = "PZA",
                    UnitPrice = unitPrice,
                    DiscountPercent = discount
                });
            }

            return list;
        }

        private static decimal RoundMoney(decimal value)
            => Math.Round(value, 2, MidpointRounding.AwayFromZero);

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

    public class QuoteLine
    {
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "PZA";
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }

        public decimal UnitPriceAfterDiscount
        {
            get
            {
                var factor = 1m - (DiscountPercent / 100m);
                return Math.Round(UnitPrice * factor, 2, MidpointRounding.AwayFromZero);
            }
        }

        public decimal LineTotal
        {
            get
            {
                var val = Quantity * UnitPriceAfterDiscount;
                return Math.Round(val, 2, MidpointRounding.AwayFromZero);
            }
        }
    }
}