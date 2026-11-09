using LaTiendita.Application.DTOs;
using LaTiendita.Domain.Entities;

namespace LaTiendita.Application.Interfaces
{
    public interface ICompraService
    {
        Task<IEnumerable<Compra>> GetAllAsync();
        Task<Compra?> GetByIdAsync(int id);
        Task<(bool Exito, string Mensaje)> RegistrarCompraAsync(CompraRegistroDto dto);
    }
}