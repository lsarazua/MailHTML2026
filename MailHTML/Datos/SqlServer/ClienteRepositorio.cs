using MailHTML.Database;
using MailHTML.Datos.Interfaces;

namespace MailHTML.Datos.SqlServer
{
    public class ClienteRepositorio : IClienteRepositorio
    {
        private readonly ASFAContext _ctx;

        public ClienteRepositorio(ASFAContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<(string Direccion, string Correo, string Telefono)> ObtenerContactoAsync(string clienteId, string addressId)
        {
            // TODO: Reemplazar por SP/tabla real
            // Por ahora, dejamos valores dummy para avanzar
            return await Task.FromResult((Direccion: $"AddressId: {addressId}",Correo: "correo@demo.com",Telefono: "999-000-0000"));
        }
    }
}