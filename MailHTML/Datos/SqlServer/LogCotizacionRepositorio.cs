using MailHTML.Database;
using MailHTML.Datos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MailHTML.Datos.SqlServer
{
    // Esta clase se encarga de registrar en la base de datos
    // lo que ocurrió cuando se intentó generar una cotización.
    // Es decir, guarda si fue exitoso o si ocurrió un error.
    public class LogCotizacionRepositorio : ILogCotizacionRepositorio
    {
        private readonly ASFAContext _ctx;

        // Recibe el contexto de base de datos para poder ejecutar consultas.
        public LogCotizacionRepositorio(ASFAContext ctx)
        {
            _ctx = ctx;
        }

        // Este método se llama cada vez que se genera (o falla) una cotización.
        // Su propósito es dejar un registro en la base de datos
        // indicando si el proceso fue exitoso o no.
        public async Task RegistrarAsync(string cotizacionId,string clienteId,int territorioId,bool exitoso,string codigo,string mensaje,string? detalle = null)
        {
            // Aquí debería guardarse la información en una tabla de log.
            // Actualmente solo ejecuta una consulta simple para validar
            // que puede comunicarse con la base de datos.

            var ok = exitoso ? 1 : 0;

            //await _ctx.Database.ExecuteSqlRawAsync(@"SELECT 1");
        }
    }
}