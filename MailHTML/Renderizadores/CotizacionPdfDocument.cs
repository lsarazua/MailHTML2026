using MailHTML.Dominio.Modelos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MailHTML.Renderizadores.Pdf
{
    public sealed class CotizacionPdfDocument : IDocument
    {
        private static readonly CultureInfo CultureMx = new CultureInfo("es-MX");

        private readonly CotizacionRenderModel _modelo;
        private readonly byte[] _logoPrincipal;
        private readonly byte[] _logoFooter;

        private enum AlineacionCelda
        {
            Izquierda,
            Centro,
            Derecha
        }

        public CotizacionPdfDocument(CotizacionRenderModel modelo, byte[] logoPrincipal = null, byte[] logoFooter = null)
        {
            _modelo = modelo ?? throw new ArgumentNullException(nameof(modelo));
            _logoPrincipal = logoPrincipal;
            _logoFooter = logoFooter;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            var encabezado = _modelo.Encabezado ?? new CotizacionEncabezadoModel();
            var partidas = _modelo.Partidas ?? new List<CotizacionPartidaModel>();
            var totales = _modelo.Totales ?? new CotizacionTotalesModel();
            var configuracion = _modelo.Configuracion ?? new CotizacionConfiguracionModel();

            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(x => ConstruirEncabezado(x, encabezado));
                page.Content().Element(x => ConstruirContenido(x, partidas, totales, configuracion));
                page.Footer().Element(x => ConstruirPiePagina(x, configuracion));
            });
        }

        private void ConstruirEncabezado(IContainer container, CotizacionEncabezadoModel encabezado)
        {
            container.Column(column =>
            {
                if (_logoPrincipal != null && _logoPrincipal.Length > 0)
                {
                    column.Item()
                        .AlignCenter()
                        .Height(60)
                        .Image(_logoPrincipal)
                        .FitArea();

                    column.Item().PaddingBottom(5);
                }

                column.Item()
                    .AlignCenter()
                    .Text("COTIZACIÓN")
                    .Bold()
                    .FontSize(17);

                column.Item()
                    .PaddingTop(12)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("Cotización para:").Bold();
                            left.Item().Text($"Nombre: {encabezado.ClienteNombre ?? string.Empty}");
                            left.Item().Text($"Dirección: {encabezado.Direccion ?? string.Empty}");
                            left.Item().Text($"Correo: {encabezado.SolicitanteCorreo ?? string.Empty}");
                            left.Item().Text($"Teléfono: {encabezado.SolicitanteTelefono ?? string.Empty}");
                        });

                        row.ConstantItem(180).AlignRight().Column(right =>
                        {
                            right.Item().AlignRight().Text($"Fecha: {encabezado.Fecha:dd/MM/yyyy}");
                            right.Item().AlignRight().Text($"N. de cotización: {encabezado.CotizacionId ?? string.Empty}");
                        });
                    });

                column.Item()
                    .PaddingTop(10)
                    .PaddingBottom(10)
                    .Text("Gracias por solicitar esta cotización. Quedamos atentos a cualquier duda o comentario y será un gusto atenderle si el producto es de su interés.");
            });
        }

        private static void ConstruirContenido(IContainer container, IReadOnlyCollection<CotizacionPartidaModel> partidas, CotizacionTotalesModel totales, CotizacionConfiguracionModel configuracion)
        {
            container.Column(column =>
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(42);
                        columns.ConstantColumn(38);
                        columns.ConstantColumn(67);
                        columns.ConstantColumn(67);
                        columns.ConstantColumn(67);
                        columns.ConstantColumn(67);
                    });

                    table.Header(header =>
                    {
                        AgregarEncabezado(header.Cell(), "Num.");
                        AgregarEncabezado(header.Cell(), "Producto");
                        AgregarEncabezado(header.Cell(), "Cant.");
                        AgregarEncabezado(header.Cell(), "Und.");
                        AgregarEncabezado(header.Cell(), "Precio Unit.");
                        AgregarEncabezado(header.Cell(), "Descuento (%)");
                        AgregarEncabezado(header.Cell(), "Precio Dcto.");
                        AgregarEncabezado(header.Cell(), "Sub total");
                    });

                    var numero = 1;

                    foreach (var partida in partidas)
                    {
                        var numeroPartida = partida.Numero > 0 ? partida.Numero : numero;

                        AgregarCelda(table.Cell(), numeroPartida.ToString(), AlineacionCelda.Centro);
                        AgregarCelda(table.Cell(), partida.ProductoNombre ?? string.Empty, AlineacionCelda.Izquierda);
                        AgregarCelda(table.Cell(), partida.Cantidad.ToString("0.##", CultureMx), AlineacionCelda.Centro);
                        AgregarCelda(table.Cell(), partida.Unidad ?? string.Empty, AlineacionCelda.Centro);
                        AgregarCelda(table.Cell(), Money(partida.PrecioUnitario), AlineacionCelda.Derecha);
                        AgregarCelda(table.Cell(), partida.DescuentoPorcentaje.ToString("0.##", CultureMx), AlineacionCelda.Centro);
                        AgregarCelda(table.Cell(), Money(partida.MontoDescuento), AlineacionCelda.Derecha);
                        AgregarCelda(table.Cell(), Money(partida.SubtotalLinea), AlineacionCelda.Derecha);

                        numero++;
                    }
                });

                column.Item()
                    .PaddingTop(8)
                    .AlignRight()
                    .Width(180)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        AgregarCeldaTotal(table.Cell(), "IVA");
                        AgregarCeldaTotal(table.Cell(), Money(totales.Iva));
                        AgregarCeldaTotal(table.Cell(), "Total:");
                        AgregarCeldaTotal(table.Cell(), Money(totales.Total));
                    });

                ConstruirTerminos(column, configuracion);
                ConstruirPago(column, configuracion);
            });
        }

        private static void ConstruirTerminos(ColumnDescriptor column, CotizacionConfiguracionModel configuracion)
        {
            var lineas = configuracion.Terminos?.Lineas?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (lineas == null || lineas.Count == 0)
                return;

            column.Item()
                .PaddingTop(14)
                .Text("Términos y condiciones:")
                .Bold();

            foreach (var linea in lineas)
            {
                column.Item()
                    .PaddingLeft(10)
                    .Text($"• {linea}");
            }
        }

        private static void ConstruirPago(ColumnDescriptor column, CotizacionConfiguracionModel configuracion)
        {
            if (string.IsNullOrWhiteSpace(configuracion.Pago?.Texto))
                return;

            column.Item()
                .PaddingTop(12)
                .Text("Formas de pago:")
                .Bold();

            column.Item()
                .PaddingTop(4)
                .Text(configuracion.Pago.Texto);
        }

        private void ConstruirPiePagina(IContainer container, CotizacionConfiguracionModel configuracion)
        {
            var footer = configuracion.Footer ?? new CotizacionFooterModel();

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    if (!string.IsNullOrWhiteSpace(footer.Empresa))
                        column.Item().Text(footer.Empresa).Bold();

                    if (!string.IsNullOrWhiteSpace(footer.Linea1))
                        column.Item().Text(footer.Linea1);

                    if (!string.IsNullOrWhiteSpace(footer.Linea2))
                        column.Item().Text(footer.Linea2);

                    if (!string.IsNullOrWhiteSpace(footer.Linea3))
                        column.Item().Text(footer.Linea3);

                    if (!string.IsNullOrWhiteSpace(footer.Correo))
                        column.Item().Text(footer.Correo);

                    if (!string.IsNullOrWhiteSpace(footer.Telefono))
                        column.Item().Text(footer.Telefono);
                });

                if (_logoFooter != null && _logoFooter.Length > 0)
                {
                    row.ConstantItem(90)
                        .AlignRight()
                        .Height(45)
                        .Image(_logoFooter)
                        .FitArea();
                }
            });
        }

        private static void AgregarEncabezado(IContainer container, string texto)
        {
            container
                .Border(1)
                .Background(Colors.Grey.Lighten2)
                .PaddingVertical(5)
                .PaddingHorizontal(3)
                .AlignCenter()
                .AlignMiddle()
                .Text(texto)
                .Bold()
                .FontSize(8);
        }

        private static void AgregarCelda(IContainer container, string texto, AlineacionCelda alineacion)
        {
            var celda = container
                .Border(1)
                .PaddingVertical(5)
                .PaddingHorizontal(3)
                .AlignMiddle();

            switch (alineacion)
            {
                case AlineacionCelda.Izquierda:
                    celda.AlignLeft().Text(texto ?? string.Empty).FontSize(8);
                    break;

                case AlineacionCelda.Derecha:
                    celda.AlignRight().Text(texto ?? string.Empty).FontSize(8);
                    break;

                default:
                    celda.AlignCenter().Text(texto ?? string.Empty).FontSize(8);
                    break;
            }
        }

        private static void AgregarCeldaTotal(IContainer container, string texto)
        {
            container
                .Border(1)
                .PaddingVertical(4)
                .PaddingHorizontal(6)
                .AlignRight()
                .Text(texto ?? string.Empty)
                .FontSize(8);
        }

        private static string Money(decimal value)
        {
            return value.ToString("C2", CultureMx);
        }
    }
}