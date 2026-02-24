using MailHTML.Dominio.Modelos;
using MailHTML.Dominio.Resultados;
using MailHTML.Negocio.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace MailHTML.Negocio
{
    // Esta clase se encarga de validar que la cotización
    // tenga la información mínima necesaria antes de generar el HTML.
    public class ValidadorCotizacion : IValidadorCotizacion
    {
        public Task<ResultadoValidacion> ValidarAsync(CotizacionRenderModel cotizacion)
        {
            // 1) Verifica que el modelo no venga vacío
            if (cotizacion == null)
                return Task.FromResult(
                    ResultadoValidacion.No("SIN_MODELO", "No se recibió modelo de cotización")
                );

            // 2) Verifica que tenga identificador
            if (string.IsNullOrWhiteSpace(cotizacion.Encabezado.CotizacionId))
                return Task.FromResult(
                    ResultadoValidacion.No("SIN_ID", "La cotización no tiene identificador")
                );

            // 3) Verifica que tenga cliente asociado
            if (string.IsNullOrWhiteSpace(cotizacion.Encabezado.ClienteId))
                return Task.FromResult(
                    ResultadoValidacion.No("SIN_CLIENTE", "La cotización no tiene cliente")
                );

            // 4) Verifica que existan productos/partidas
            if (cotizacion.Partidas == null || cotizacion.Partidas.Count == 0)
                return Task.FromResult(
                    ResultadoValidacion.No("SIN_PARTIDAS", "La cotización no tiene productos/partidas")
                );

            // 5) Verifica que ninguna partida tenga cantidad inválida
            if (cotizacion.Partidas.Any(p => p.Cantidad <= 0))
                return Task.FromResult(
                    ResultadoValidacion.No("CANTIDAD_INVALIDA", "Hay partidas con cantidad inválida")
                );

            // 6) Verifica que ningún precio sea negativo
            if (cotizacion.Partidas.Any(p => p.PrecioUnitario < 0))
                return Task.FromResult(
                    ResultadoValidacion.No("PRECIO_INVALIDO", "Hay partidas con precio inválido")
                );

            // Si pasa todas las validaciones, la cotización está lista para imprimirse
            return Task.FromResult(
                ResultadoValidacion.Ok("Cotización válida para impresión")
            );
        }
    }
}