using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Datos.Interfaces
{
    public interface ILogCotizacionRepositorio
    {
        Task RegistrarAsync(string cotizacionId,string clienteId,int territorioId, bool exitoso, string codigo,string mensaje, string? detalle = null );
    }
}
