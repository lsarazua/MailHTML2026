using MailHTML.Datos.Interfaces;
using MailHTML.Dominio.Modelos;
using MailHTML.Dominio.Resultados;
using MailHTML.Negocio.Interfaces;
using MailHTML.Renderizadores;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MailHTML.Negocio
{
    public class ServicioImpresionCotizacion : IServicioImpresionCotizacion
    {
        private const decimal IvaRate = 0.16m;

        private readonly ICotizacionRepositorio _cotizacionRepo;                // Obtiene encabezado y partidas de la cotización desde BD
        private readonly IClienteRepositorio _clienteRepo;                      // Obtiene datos de contacto/dirección del cliente desde BD
        private readonly IConfiguracionCotizacionRepositorio _configRepo;       // Obtiene configuración para imprimir/renderizar (por territorio, etc.)
        private readonly ILogCotizacionRepositorio _logRepo;                    // Registra auditoría (éxito/falla, motivo, detalle)
        private readonly IValidadorCotizacion _validador;                       // Aplica reglas de negocio para permitir o no la impresión
        private readonly IRenderizadorHtmlCotizacion _renderizador;             // Genera el HTML final con el layout (fijo/dinámico)

        public ServicioImpresionCotizacion(ICotizacionRepositorio cotizacionRepo,IClienteRepositorio clienteRepo,IConfiguracionCotizacionRepositorio configRepo,ILogCotizacionRepositorio logRepo,      IValidadorCotizacion validador,IRenderizadorHtmlCotizacion renderizador)
        {
           
            _cotizacionRepo = cotizacionRepo;
            _clienteRepo = clienteRepo;
            _configRepo = configRepo;
            _logRepo = logRepo;
            _validador = validador;
            _renderizador = renderizador;
        }

        public async Task<ResultadoImpresionCotizacion> ImprimirAsync(string cotizacionId)
        {
            // Modelo “contenedor” que se arma con datos + configuración + totales para renderizar
            var cot = new CotizacionRenderModel();

            try
            {
                // 1) Obtiene datos base de la cotización (encabezado + partidas)
                var encabezado = await _cotizacionRepo.ObtenerEncabezadoAsync(cotizacionId);
                var partidas = await _cotizacionRepo.ObtenerPartidasAsync(cotizacionId);

                // 2) Completa datos de contacto que se muestran en el documento (dirección/correo/teléfono)
                var contacto = await _clienteRepo.ObtenerContactoAsync(encabezado.ClienteId, encabezado.Direccion);
                encabezado.Direccion = contacto.Direccion;
                encabezado.SolicitanteCorreo = contacto.Correo;
                encabezado.SolicitanteTelefono = contacto.Telefono;

                // 3) Obtiene configuración de impresión/render (por territorio)
                var config = await _configRepo.ObtenerAsync(encabezado.TerritorioId);

                // 4) Arma el modelo final a renderizar
                cot.Encabezado = encabezado;
                cot.Partidas = partidas;
                cot.Configuracion = config;

                // 5) Calcula totales (Subtotal, IVA, Total)
                var subtotal = cot.Partidas.Sum(x => x.SubtotalLinea);
                var iva = Math.Round(subtotal * IvaRate, 2, MidpointRounding.AwayFromZero);
                var total = Math.Round(subtotal + iva, 2, MidpointRounding.AwayFromZero);

                cot.Totales = new CotizacionTotalesModel
                {
                    Subtotal = subtotal,
                    Iva = iva,
                    Total = total
                };

                // 6) Valida reglas de negocio antes de generar HTML
                var validacion = await _validador.ValidarAsync(cot);

                if (!validacion.Permitido)
                {
                    // 7) Si no se permite, registra auditoría y regresa resultado “No”
                    await _logRepo.RegistrarAsync(cotizacionId: cotizacionId, clienteId: encabezado.ClienteId, territorioId: encabezado.TerritorioId,exitoso: false, codigo: validacion.Codigo,
                        mensaje: validacion.Mensaje,detalle: string.Join(" | ", validacion.Detalles));

                    return ResultadoImpresionCotizacion.No(cotizacionId: cotizacionId,clienteId: encabezado.ClienteId,territorioId: encabezado.TerritorioId,validacion: validacion,mensaje: validacion.Mensaje);
                }

                // 8) Genera el HTML final usando el renderizador (layout fijo/dinámico)
                var html = _renderizador.Renderizar(cot);

                // 9) Registra auditoría de éxito
                await _logRepo.RegistrarAsync(cotizacionId: cotizacionId,clienteId: encabezado.ClienteId,territorioId: encabezado.TerritorioId,exitoso: true,codigo: "OK",mensaje: "Cotización generada",
                    detalle: $"Partidas:{cot.Partidas.Count}");

                // 10) Regresa resultado OK con HTML y modelo usado
                return ResultadoImpresionCotizacion.Ok(cotizacionId: cotizacionId,clienteId: encabezado.ClienteId,territorioId: encabezado.TerritorioId,html: html,modeloRender: cot);
            }
            catch (Exception ex)
            {
                // Manejo de error: intenta recuperar ids si alcanzaron a cargarse para log
                var terr = cot?.Encabezado?.TerritorioId ?? 0;
                var cli = cot?.Encabezado?.ClienteId ?? "";

                // Log de excepción
                await _logRepo.RegistrarAsync(cotizacionId: cotizacionId,clienteId: cli,territorioId: terr,exitoso: false,codigo: "EXCEPTION",mensaje: "Error al generar cotización",detalle: ex.Message);

                // Resultado de error controlado
                return ResultadoImpresionCotizacion.No(cotizacionId: cotizacionId,clienteId: cli,territorioId: terr, 
                    validacion: ResultadoValidacion.No("EXCEPTION", "Ocurrió un error al generar la cotización", ex.Message),
                    mensaje: ex.Message
                );
            }
        }
    }
}