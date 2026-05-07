using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Dominio.Modelos
{
    public class ArchivoAdjuntoRenderModel
    {
        public string NombreArchivo { get; set; }
        public string ContentType { get; set; }
        public byte[] Contenido { get; set; }
    }
}
