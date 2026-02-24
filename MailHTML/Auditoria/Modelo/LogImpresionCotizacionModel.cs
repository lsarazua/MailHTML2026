namespace MailHTML.Auditoria.Modelo
{
    public class LogImpresionCotizacionModel
    {
        public int Id { get; set; }

        public string CotizacionId { get; set; } = "";
        public string ClienteId { get; set; } = "";
        public int TerritorioId { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        public bool Exitoso { get; set; }

        // Código técnico: OK, SIN_PARTIDAS, EXCEPTION, etc.
        public string Codigo { get; set; } = "";

        // Mensaje legible
        public string Mensaje { get; set; } = "";

        // Información adicional (stacktrace, detalle validación, etc.)
        public string? Detalle { get; set; }

        // Opcional: para trazabilidad futura
        public string? Usuario { get; set; }

        // Opcional: tamaño del HTML generado
        public int? TamanoHtml { get; set; }
    }
}