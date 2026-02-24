using MailHTML.Dominio.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Dominio.Resultados
{
    public class ResultadoImpresionCotizacion
    {
        public bool Exitoso { get; set; }

        // El resultado de reglas (si no pasa, aquí viene el motivo)
        public ResultadoValidacion Validacion { get; set; } = ResultadoValidacion.No("SIN_VALIDAR", "No se ejecutó validación");

        // HTML generado (solo si Exitoso = true)
        public string Html { get; set; } = "";

        // Modelo “cocinado” que se usó para render (útil para auditoría o debug)
        public CotizacionRenderModel? ModeloRender { get; set; }

        // Identificadores básicos para logging / trazabilidad
        public string CotizacionId { get; set; } = "";
        public string ClienteId { get; set; } = "";
        public int TerritorioId { get; set; }

        // Mensaje final (para UI / logs)
        public string Mensaje { get; set; } = "";

        public static ResultadoImpresionCotizacion Ok(
            string cotizacionId,
            string clienteId,
            int territorioId,
            string html,
            CotizacionRenderModel? modeloRender = null,
            string mensaje = "Cotización generada correctamente")
            => new ResultadoImpresionCotizacion
            {
                Exitoso = true,
                Validacion = ResultadoValidacion.Ok(),
                CotizacionId = cotizacionId ?? "",
                ClienteId = clienteId ?? "",
                TerritorioId = territorioId,
                Html = html ?? "",
                ModeloRender = modeloRender,
                Mensaje = mensaje ?? ""
            };

        public static ResultadoImpresionCotizacion No(
            string cotizacionId,
            string clienteId,
            int territorioId,
            ResultadoValidacion validacion,
            string mensaje = "No fue posible generar la cotización")
            => new ResultadoImpresionCotizacion
            {
                Exitoso = false,
                Validacion = validacion ?? ResultadoValidacion.No("ERROR", "Validación inválida"),
                CotizacionId = cotizacionId ?? "",
                ClienteId = clienteId ?? "",
                TerritorioId = territorioId,
                Html = "",
                ModeloRender = null,
                Mensaje = mensaje ?? ""
            };
    }
}
