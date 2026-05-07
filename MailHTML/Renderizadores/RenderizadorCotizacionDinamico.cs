using MailHTML.Dominio.Modelos;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace MailHTML.Renderizadores
{
    public class RenderizadorCotizacionDinamico
    {
        private readonly IConfiguration _configuration;

        public RenderizadorCotizacionDinamico(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ArchivoAdjuntoRenderModel> GenerarArchivoAsync(CotizacionRenderModel modelo, string cotizacionId)
        {
            var tipoRender = _configuration["RenderSettings:TipoRenderizador"];

            if (string.IsNullOrWhiteSpace(tipoRender))
                throw new Exception("No existe configuración RenderSettings:TipoRenderizador.");

            var type = Type.GetType(tipoRender);
            if (type == null)
                throw new Exception($"No se encontró el tipo configurado: {tipoRender}");

            if (!typeof(ICotizacionRenderizador).IsAssignableFrom(type))
                throw new Exception($"El tipo {tipoRender} no implementa ICotizacionRenderizador.");

            var instancia = Activator.CreateInstance(type) as ICotizacionRenderizador;
            if (instancia == null)
                throw new Exception($"No fue posible crear una instancia de {tipoRender}");

            return await instancia.GenerarArchivoAsync(modelo, cotizacionId);
        }
    }
}