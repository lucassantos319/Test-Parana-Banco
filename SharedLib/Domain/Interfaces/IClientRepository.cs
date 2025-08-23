
using SharedLib.Domain.DTOs;

namespace SharedLib.Domain.Interfaces
{
    public interface IClientRepository
    {
        Task<List<ClientDto>> GetAllAsync();
        Task<ClientDto> GetByIdAsync(Guid id);
        Task<ClientDto> GetByEmailAsync(string email);
        Task<ClientDto> CreateAsync(ClientDto client);
        Task<ClientDto> UpdateAsync(ClientDto client);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByEmailAsync(string email);
    }
}
