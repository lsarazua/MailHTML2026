using System;
using System.Collections.Generic;
using System.Linq;

namespace MailHTML.Dominio.Resultados
{
    // Esta clase representa el resultado de una validación.
    // Indica si se permite continuar (por ejemplo, imprimir una cotización)
    // o si algo falló y debe detenerse el proceso.
    public class ResultadoValidacion
    {
       
        public bool Permitido { get; set; }
        public string Codigo { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public List<string> Detalles { get; set; } = new();

        // Marca el resultado como permitido.
        public static ResultadoValidacion Ok(string mensaje = "OK")
            => new ResultadoValidacion
            {
                Permitido = true,
                Codigo = "OK",
                Mensaje = mensaje
            };
        public static ResultadoValidacion No(string codigo, string mensaje, params string[] detalles)
            => new ResultadoValidacion
            {
                Permitido = false,
                Codigo = codigo ?? "",
                Mensaje = mensaje ?? "",
                Detalles = detalles?.ToList() ?? new List<string>()
            };
    }
}