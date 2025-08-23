using SharedLib.Domain.DTOs;
using SharedLib.Domain.Entities;
using SharedLib.Domain.Requests;

namespace API.Services.Interfaces
{
    public interface IClientService
    {
        Task<Result<bool>> CreateClientAsync(ClientRequestBody requestBody);
        Task<Result<ClientDto>> GetClientByIdAsync(Guid id);
        Task<Result<IEnumerable<ClientDto>>> GetAllClientsAsync();
        Task<Result<ClientDto>> UpdateClientAsync(Guid id, ClientRequestBody requestBody);
        Task<Result<bool>> DeleteClientAsync(Guid id);
    }
}
