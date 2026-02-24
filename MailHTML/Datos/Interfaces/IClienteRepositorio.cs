using System;
using System.Collections.Generic;
using System.Text;

namespace MailHTML.Datos.Interfaces
{
    public interface IClienteRepositorio
    {
        Task<(string Direccion, string Correo, string Telefono)> ObtenerContactoAsync(string clienteId, string addressId);
    }
}
