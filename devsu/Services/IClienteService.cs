using System.Collections.Generic;
using System.Threading.Tasks;
using devsu.DTOs;

namespace devsu.Services
{
    public interface IClienteService
    {
        Task<ClienteDto> GetClienteByIdAsync(int id);
        Task<ClienteDto> CreateClienteAsync(ClienteDto clienteDto);
        Task<IEnumerable<ClienteDto>> GetAllClientesAsync();
        Task<PaginatedResponse<ClienteDto>> GetAllClientesPaginatedAsync(PaginationParameters paginationParameters);
        Task<PaginatedResponse<ClienteDto>> GetClientesFilteredAsync(ClienteFilterDto filterDto);
        Task<ClienteDto> UpdateClienteAsync(int id, ClienteDto clienteDto);
        Task<ClienteDto> PatchClienteAsync(int id, ClientePatchDto clientePatchDto);
        Task<bool> DeleteClienteAsync(int id);
    }
}