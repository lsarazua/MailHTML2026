
using MailHTML.Database;
using MailHTML.Datos.Interfaces;
using MailHTML.Dominio.Modelos;
using Microsoft.EntityFrameworkCore;

namespace MailHTML.Datos.SqlServer
{
    public class CotizacionRepositorio : ICotizacionRepositorio
    {
        private readonly ASFAContext _ctx;

        public CotizacionRepositorio(ASFAContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<CotizacionEncabezadoModel> ObtenerEncabezadoAsync(string cotizacionId)
        {
            // TODO: Reemplazar por SP/tabla real
            // Por ahora validamos que la DB responde con un "dummy" útil
            var fecha = await _ctx.Database
                .SqlQueryRaw<DateTime>("SELECT GETDATE()")
                .FirstAsync();

            return new CotizacionEncabezadoModel
            {
                CotizacionId = cotizacionId,
                Fecha = fecha
            };
        }

        public async Task<List<CotizacionPartidaModel>> ObtenerPartidasAsync(string cotizacionId)
        {
            // TODO: Reemplazar por SP/tabla real
            // Por ahora devolvemos una lista fake para validar el pipeline
            return await Task.FromResult(new List<CotizacionPartidaModel>
            {
                new CotizacionPartidaModel
                {
                    Numero = 1,
                    ProductoId = "P001",
                    ProductoNombre = "Producto de prueba 1",
                    Cantidad = 2,
                    Unidad = "PZA",
                    PrecioUnitario = 120.50m,
                    DescuentoPorcentaje = 0
                },
                new CotizacionPartidaModel
                {
                    Numero = 2,
                    ProductoId = "P002",
                    ProductoNombre = "Producto de prueba 2",
                    Cantidad = 1,
                    Unidad = "PZA",
                    PrecioUnitario = 250m,
                    DescuentoPorcentaje = 10
                }
            });
        }
    }
}