using SharedLib.Domain.DTOs;
using SharedLib.Domain.Interfaces;
using System.Text.Json;

namespace SharedLib.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly string _filePath;
        private readonly object _lockObject = new object();

        public ClientRepository()
        {
            _filePath = Path.Combine("C:\\Users\\lucas.ssantos\\source\\repos\\Test-Parana-Banco\\SharedLib\\Data\\MockData", "ClientsData.json");
            
            // Garantir que o diretório existe
            var directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Criar arquivo se não existir
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        public async Task<List<ClientDto>> GetAllAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                var clients = JsonSerializer.Deserialize<List<ClientDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return clients.ToList() ?? new List<ClientDto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar todos os clientes: {ex.Message}", ex);
            }
        }

        public async Task<ClientDto> GetByIdAsync(Guid id)
        {
            try
            {
                var clients = await GetAllAsync();
                return clients.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar cliente por ID: {ex.Message}", ex);
            }
        }

        public async Task<ClientDto> GetByEmailAsync(string email)
        {
            try
            {
                var clients = await GetAllAsync();
                return clients.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar cliente por email: {ex.Message}", ex);
            }
        }

        public async Task<ClientDto> CreateAsync(ClientDto client)
        {
            try
            {
                var clients = await GetAllAsync();
                
                lock (_lockObject)
                {
                    // Verificar se o email já existe
                    if (clients.Any(c => c.Email.Equals(client.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException($"Cliente com email {client.Email} já existe.");
                    }
                    
                    // Definir ID se não fornecido
                    if (client.Id == Guid.Empty)
                    {
                        client.Id = Guid.NewGuid();
                    }
                    
                    // Definir timestamps
                    client.CreatedAt = DateTime.UtcNow;
                    client.UpdatedAt = DateTime.UtcNow;
                    
                    clients.Add(client);
                    
                    var json = JsonSerializer.Serialize(clients, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    
                    File.WriteAllText(_filePath, json);
                }
                
                return client;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar cliente: {ex.Message}", ex);
            }
        }

        public async Task<ClientDto> UpdateAsync(ClientDto client)
        {
            try
            {
                lock (_lockObject)
                {
                    var clients = GetAllAsync().Result.ToList();
                    var existingClient = clients.FirstOrDefault(c => c.Id == client.Id);
                    
                    if (existingClient == null)
                    {
                        throw new InvalidOperationException($"Cliente com ID {client.Id} não encontrado.");
                    }
                    
                    // Verificar se o email já existe em outro cliente
                    var clientWithSameEmail = clients.FirstOrDefault(c => 
                        c.Id != client.Id && 
                        c.Email.Equals(client.Email, StringComparison.OrdinalIgnoreCase));
                    
                    if (clientWithSameEmail != null)
                    {
                        throw new InvalidOperationException($"Cliente com email {client.Email} já existe.");
                    }
                    
                    // Atualizar propriedades
                    existingClient.Name = client.Name;
                    existingClient.Email = client.Email;
                    existingClient.PhoneNumber = client.PhoneNumber;
                    existingClient.Status = client.Status;
                    existingClient.Type = client.Type;
                    existingClient.Balance = client.Balance;
                    existingClient.IncomeAmount = client.IncomeAmount;
                    existingClient.Limit = client.Limit;
                    existingClient.AddicionalCreditLimit = client.AddicionalCreditLimit;
                    existingClient.AddicionalCreditLimitStatus = client.AddicionalCreditLimitStatus;
                    existingClient.UpdatedAt = DateTime.UtcNow;
                    
                    var json = JsonSerializer.Serialize(clients, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    
                    File.WriteAllText(_filePath, json);
                }
                
                return client;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar cliente: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                lock (_lockObject)
                {
                    var clients = GetAllAsync().Result.ToList();
                    var client = clients.FirstOrDefault(c => c.Id == id);
                    
                    if (client == null)
                    {
                        return false;
                    }
                    
                    clients.Remove(client);
                    
                    var json = JsonSerializer.Serialize(clients, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    
                    File.WriteAllText(_filePath, json);
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao deletar cliente: {ex.Message}", ex);
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                var client = await GetByIdAsync(id);
                return client != null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao verificar existência do cliente: {ex.Message}", ex);
            }
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            try
            {
                var client = await GetByEmailAsync(email);
                return client != null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao verificar existência do cliente por email: {ex.Message}", ex);
            }
        }
    }
}
