using MailHTML.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace MailHTMLTest
{
    [TestClass]
    public class ASFAContextConnectionTests
    {
        private Microsoft.Extensions.Configuration.IConfiguration _configuration;

        [TestInitialize]
        public void Setup()
        {
            _configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(System.AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        [TestMethod]
        public async Task EF_Should_Connect_Successfully()
        {
            try
            {
                using var ctx = new ASFAContext(_configuration);

                // Esto normalmente da el detalle real si falla
                await ctx.Database.OpenConnectionAsync();

                // Si abrió, cerramos
                await ctx.Database.CloseConnectionAsync();

                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail($"No se pudo conectar. Detalle: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task EF_Should_Execute_Query_And_Return_Data()
        {
            using var ctx = new ASFAContext(_configuration);

            var result = await ctx.DbPing
                .FromSqlRaw("SELECT GETDATE() AS ServerDate")
                .AsNoTracking()
                .FirstOrDefaultAsync();

            Assert.IsNotNull(result, "La consulta no devolvió resultados");
            Assert.IsTrue(result.ServerDate.Year > 2000, "La fecha devuelta por SQL no es válida");
        }

        [TestMethod]
        public async Task EF_Should_Read_Layout_From_DB()
        {
            using var ctx = new ASFAContext(_configuration);

            var layout = await ctx.LayoutsMailWeb
                .FromSqlRaw("SELECT TOP 1 Layoutsmail, ID, Nombre, Titulo, Asunto, Texto, Body, Piepagina FROM LayoutsmailWeb")
                .AsNoTracking()
                .FirstOrDefaultAsync();

            Assert.IsNotNull(layout, "No se encontró ningún layout en LayoutsmailWeb");
            Assert.IsFalse(string.IsNullOrWhiteSpace(layout.Body), "El layout no trae Body (HTML)");
        }
    }
}