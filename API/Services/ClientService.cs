using API.Services.Interfaces;
using SharedLib.Domain.DTOs;
using SharedLib.Domain.Entities;
using SharedLib.Domain.Enums;
using SharedLib.Domain.Interfaces;
using SharedLib.Domain.Interfaces.Bus;
using SharedLib.Domain.Requests;
using SharedLib.Infrastructure;

namespace API.Services
{
    public class ClientService : IClientService
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<ClientService> _logger;
        private readonly IClientRepository _repository;

        public ClientService(IEventBus eventBus, ILogger<ClientService> logger, IClientRepository repository)
        {
            _logger = logger;
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus), "Event bus cannot be null.");
            _repository = repository ?? throw new ArgumentNullException(nameof(repository), "Repository cannot be null.");
        }
        
        public async Task<Result<bool>> CreateClientAsync(ClientRequestBody client)
        {
            try
            {
                var validClient = ValidateClient(client);
                if (!validClient.IsSuccess)
                {
                    _logger.LogWarning("Client validation failed: {Message}", validClient.Message);
                    return Result<bool>.Failure(validClient.Message);
                }

                // Verificar se o email já existe
                var existingClient = await _repository.GetByEmailAsync(client.Email);
                if (existingClient != null)
                {
                    return Result<bool>.Failure($"Cliente com email {client.Email} já existe.");
                }

                // Converter para ClientDto
                var clientDto = new ClientDto
                {
                    Name = client.Name,
                    Email = client.Email,
                    PhoneNumber = client.PhoneNumber,
                    Status = StatusClientEnum.Active,
                    Type = client.Type,
                    Balance = client.Balance,
                    IncomeAmount = client.Balance,
                };

                // Salvar no repositório
                var savedClient = await _repository.CreateAsync(clientDto);

                // Enviar eventos baseado no tipo de conta
                if (client.Type == AccountTypeEnum.Current && client.Balance > 1000)
                {
                    var resultCreditCard = SendCreditCardAsync(savedClient);
                    if (!resultCreditCard.IsSuccess)
                    {
                        _logger.LogWarning("Error to send message: {Message}", resultCreditCard.Message);
                        return Result<bool>.Failure(resultCreditCard.Message);
                    } 
                }
                else
                {
                    var resultCreditProposal = SendCreditProposalAsync(savedClient);
                    if (!resultCreditProposal.IsSuccess)
                    {
                        _logger.LogWarning("Error to send message: {Message}", resultCreditProposal.Message);
                        return Result<bool>.Failure(resultCreditProposal.Message);
                    }
                }

                return Result<bool>.Success(true, "Cliente criado com sucesso.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return Result<bool>.Failure("An error occurred while creating the client.");
            }
        }

        private Result<bool> SendCreditProposalAsync(ClientDto client)
        {
            try
            {
                var @event = new CreditProposalEvent(client.Id, client.IncomeAmount);
                _eventBus.Publish(@event);

                return Result<bool>.Success(true, "Credit proposal message sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending credit proposal");
                return Result<bool>.Failure("An error occurred while sending the credit proposal.");
            }
        }

        private Result<bool> SendCreditCardAsync(ClientDto client)
        {
            try
            {
                var @event = new CreditCardCreationEvent(client.Id);
                _eventBus.Publish(@event);

                return Result<bool>.Success(true, "Credit card creation message sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending credit card creation");
                return Result<bool>.Failure("An error occurred while sending the credit card creation.");
            }
        }

        public async Task<Result<ClientDto>> GetClientByIdAsync(Guid id)
        {
            try
            {
                var client = await _repository.GetByIdAsync(id);
                if (client == null)
                {
                    return Result<ClientDto>.Failure($"Cliente com ID {id} não encontrado.");
                }

                return Result<ClientDto>.Success(client, "Cliente encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client by ID");
                return Result<ClientDto>.Failure("An error occurred while getting the client.");
            }
        }

        public async Task<Result<IEnumerable<ClientDto>>> GetAllClientsAsync()
        {
            try
            {
                var clients = await _repository.GetAllAsync();
                return Result<IEnumerable<ClientDto>>.Success(clients, "Clientes encontrados com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all clients");
                return Result<IEnumerable<ClientDto>>.Failure("An error occurred while getting the clients.");
            }
        }

        public async Task<Result<ClientDto>> UpdateClientAsync(Guid id, ClientRequestBody requestBody)
        {
            try
            {
                var validClient = ValidateClient(requestBody);
                if (!validClient.IsSuccess)
                {
                    _logger.LogWarning("Client validation failed: {Message}", validClient.Message);
                    return Result<ClientDto>.Failure(validClient.Message);
                }

                var existingClient = await _repository.GetByIdAsync(id);
                if (existingClient == null)
                {
                    return Result<ClientDto>.Failure($"Cliente com ID {id} não encontrado.");
                }

                // Verificar se o email já existe em outro cliente
                var clientWithSameEmail = await _repository.GetByEmailAsync(requestBody.Email);
                if (clientWithSameEmail != null && clientWithSameEmail.Id != id)
                {
                    return Result<ClientDto>.Failure($"Cliente com email {requestBody.Email} já existe.");
                }

                // Atualizar propriedades
                existingClient.Name = requestBody.Name;
                existingClient.Email = requestBody.Email;
                existingClient.PhoneNumber = requestBody.PhoneNumber;
                existingClient.Type = requestBody.Type;
                existingClient.Balance = requestBody.Balance;
                existingClient.UpdatedAt = DateTime.UtcNow;

                var updatedClient = await _repository.UpdateAsync(existingClient);
                return Result<ClientDto>.Success(updatedClient, "Cliente atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client");
                return Result<ClientDto>.Failure("An error occurred while updating the client.");
            }
        }

        public async Task<Result<bool>> DeleteClientAsync(Guid id)
        {
            try
            {
                var existingClient = await _repository.GetByIdAsync(id);
                if (existingClient == null)
                {
                    return Result<bool>.Failure($"Cliente com ID {id} não encontrado.");
                }

                var deleted = await _repository.DeleteAsync(id);
                if (deleted)
                {
                    return Result<bool>.Success(true, "Cliente deletado com sucesso.");
                }
                else
                {
                    return Result<bool>.Failure("Erro ao deletar cliente.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client");
                return Result<bool>.Failure("An error occurred while deleting the client.");
            }
        }

        private Result<bool> ValidateClient(ClientRequestBody client)
        {
            if ( string.IsNullOrEmpty(client.Name) || 
                 string.IsNullOrEmpty(client.Email) || 
                 string.IsNullOrEmpty(client.PhoneNumber))
            {
                return Result<bool>.Failure("Client name, email, and phone number are required.");
            }

            if (!client.Email.Contains("@"))
            {
                return Result<bool>.Failure("Invalid email format.");
            }

            if (client.PhoneNumber.Length < 10)
            {
                return Result<bool>.Failure("Phone number must be at least 10 digits long.");
            }

            return Result<bool>.Success(true, "Client validation successful.");
        }

    }
}
