using MailHTML.Services;
using MailHTML.Modelos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Kangaroo.MailTemplates.TestsH
{
    [TestClass]
    public class SupportEmailTemplateRendererFijoTests
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
        [DataRow(30)]
        [DataRow(45)]
        [DataRow(60)]
        [DataRow(100)]
        public void Render_HTML_Fijo(int productRows)
        {
            var model = new SupportEmailModel
            {
                CustomerName = "Cliente Prueba",
                CustomerId = "C123",
                AddressId = "A456",
                TerritoryId = 10,
                RequesterName = "luis@correo.com",
                RequesterEmployeeId = 999
            };

            var renderer = new SupportEmailTemplateRendererFijo();
            var html = renderer.Render(model, productRows);

            var directory = @"C:\temp";
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, $"cotizacion_fixed_PruebaFinal_{productRows}.html");
            File.WriteAllText(filePath, html);

            Assert.IsFalse(string.IsNullOrWhiteSpace(html));
            Assert.IsTrue(html.Contains("COTIZACIÓN", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.Contains("Nombre: Cliente Prueba", StringComparison.OrdinalIgnoreCase));
        }
    }
}