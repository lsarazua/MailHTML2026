using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Dominio.Modelos
{
    public class LayoutMailModel
    {
        public int Layoutsmail { get; set; }         
        public string? ID { get; set; }               
        public string? Nombre { get; set; }           
        public string? Titulo { get; set; }          
        public string? Asunto { get; set; }           
        public string? Texto { get; set; }            
        public string? Body { get; set; }             
        public string? Piepagina { get; set; }       
    }
}
