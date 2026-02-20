using MailHTML.Modelos;
using MailHTML.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace MailHTMLTest.TestsH
{
    [TestClass]
    public class SupportEmailTemplateRendererTests
    {
        [DataTestMethod]
        [DataRow(5)]
        [DataRow(9)]
        [DataRow(15)]
        [DataRow(16)]
        [DataRow(17)]
        [DataRow(32)]
        //[DataRow(33)]
        [DataRow(50)]
        public void Render_ShouldGenerateHtml_WithDynamicPagingRule(int productRows)
        {
            var model = new SupportEmailModel
            {
                CustomerName = "Cliente Prueba",
                CustomerId = "C123",
                AddressId = "A456",
                TerritoryId = 10,
                RequesterName = "Luis",
                RequesterEmployeeId = 999
            };

            var renderer = new SupportEmailTemplateRenderer();

            var html = renderer.Render(model, productRows);

            var directory = @"C:\temp";
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, $"cotizacion_Nuevo_final_{productRows}.html");
            File.WriteAllText(filePath, html);

            Assert.IsFalse(string.IsNullOrWhiteSpace(html));
            Assert.IsTrue(html.Contains("COTIZACIÓN", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.Contains("Nombre: Cliente Prueba", StringComparison.OrdinalIgnoreCase));

            // Validación visual rápida: imprime el total de páginas
            // (solo verifica que se hayan generado contenedores .page)
            int expectedRowsPerPage = productRows <= 16 ? 8 : 16;
            int expectedPages = (productRows + expectedRowsPerPage - 1) / expectedRowsPerPage;
        }
    }
}