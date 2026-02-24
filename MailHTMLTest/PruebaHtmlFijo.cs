using MailHTML.Dominio.Modelos;
using MailHTML.Renderizadores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MailHTMLTest
{
    [TestClass]
    public class PruebaHtmlFijo
    {
        [DataTestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(5)]
        [DataRow(8)]
        [DataRow(10)]
        [DataRow(11)]
        [DataRow(12)]
        [DataRow(15)]
        [DataRow(17)]
        [DataRow(20)]
        [DataRow(25)]
        public void Render_HTML_Fijo(int productRows)
        {
            var modelo = GenerarModeloPrueba(productRows);

            var renderer = new SupportEmailTemplateRendererFijo();
            var html = renderer.Render(modelo);

            var directory = @"C:\temp";
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, $"Cotizacion23_02_2026_{productRows}.html");
            File.WriteAllText(filePath, html);

            Assert.IsFalse(string.IsNullOrWhiteSpace(html));
            Assert.IsTrue(html.Contains("COTIZACIÓN", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.Contains("Cliente Prueba", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(html.Contains("Término y condiciones", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.Contains("Formas de pago", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.Contains("BANORTE", StringComparison.OrdinalIgnoreCase));
        }

        private CotizacionRenderModel GenerarModeloPrueba(int cantidadProductos)
        {
            var encabezado = new CotizacionEncabezadoModel
            {
                CotizacionId = "COT-001",
                Fecha = DateTime.Now,
                ClienteId = "C123",
                ClienteNombre = "Cliente Prueba",
                Direccion = "Dirección de prueba 123",
                TerritorioId = 10,
                SolicitanteNombre = "Luis",
                SolicitanteEmpleadoId = 999,
                SolicitanteCorreo = "luis@correo.com",
                SolicitanteTelefono = "9991234567"
            };

            var partidas = new List<CotizacionPartidaModel>();
            for (int i = 1; i <= cantidadProductos; i++)
            {
                partidas.Add(new CotizacionPartidaModel
                {
                    Numero = i,
                    ProductoId = $"P{i}",
                    ProductoNombre = $"Producto #{i} descripción ejemplo",
                    Cantidad = (i % 3) + 1,
                    Unidad = "PZA",
                    PrecioUnitario = 150 + i,
                    DescuentoPorcentaje = (i % 4) * 5
                });
            }

            decimal subtotal = partidas.Sum(x => x.SubtotalLinea);
            decimal iva = Math.Round(subtotal * 0.16m, 2, MidpointRounding.AwayFromZero);
            decimal total = subtotal + iva;

            var totales = new CotizacionTotalesModel
            {
                Subtotal = subtotal,
                Iva = iva,
                Total = total
            };

            var configuracion = new CotizacionConfiguracionModel
            {
                RenglonesPorHoja = 5,
                UrlLogoPrincipal = "https://www.kangaroocrm.net/KangarooCrm/Logo_AllianceSFA.png",
                UrlLogoFooter = "https://www.kangaroocrm.net/kangaroocrm/LOGO_KANGAROO_CRM.PNG",
                Terminos = new CotizacionTerminosModel
                {
                    Lineas = new List<string>
                    {
                        "La presente cotización tiene una vigencia de 15 días naturales.",
                        "Los precios están sujetos a cambios sin previo aviso.",
                        "El pedido se confirmará al recibir el pago o anticipo."
                    }
                },
                Pago = new CotizacionPagoModel
                {
                    Texto =
                        "Pago mediante transferencia bancaria.\r\n" +
                        "Pago con tarjetas de crédito y/o débito.\r\n" +
                        "BANCO: BANCO MERCANTIL DEL NORTE SA (BANORTE)\r\n" +
                        "CLABE: 072910008923603963\r\n" +
                        "***Favor de enviar comprobante de pago al correo: contacto@ftppeninsular.com\r\n" +
                        "Los pedidos con más de 5 días de cotización y sin pago realizado serán cancelados."
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

            return new CotizacionRenderModel
            {
                Encabezado = encabezado,
                Partidas = partidas,
                Totales = totales,
                Configuracion = configuracion
            };
        }
    }
}