# Repositório de Clientes - Documentação

## Visão Geral

O repositório de clientes foi implementado para gerenciar operações CRUD (Create, Read, Update, Delete) usando um arquivo JSON como fonte de dados mockada.

## Funcionalidades Implementadas

### 1. Interface IClientRepository

```csharp
public interface IClientRepository
{
    Task<IEnumerable<ClientDto>> GetAllAsync();
    Task<ClientDto> GetByIdAsync(Guid id);
    Task<ClientDto> GetByEmailAsync(string email);
    Task<ClientDto> CreateAsync(ClientDto client);
    Task<ClientDto> UpdateAsync(ClientDto client);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email);
}
```

### 2. Implementação ClientRepository

O repositório implementa:
- **Thread-safe**: Usa lock para operações de escrita
- **Validações**: Verifica duplicação de email
- **Tratamento de erros**: Exceções específicas para cada operação
- **Persistência**: Salva automaticamente no arquivo JSON

## Endpoints da API

### POST /api/client
Cria um novo cliente

**Request Body:**
```json
{
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "phoneNumber": "11987654321",
  "type": 1,
  "balance": 2500.00,
  "incomeAmount": 5000.00
}
```

### GET /api/client
Lista todos os clientes

### GET /api/client/{id}
Busca cliente por ID

### PUT /api/client/{id}
Atualiza cliente existente

### DELETE /api/client/{id}
Remove cliente

## Estrutura dos Dados

### ClientDto
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "phoneNumber": "11987654321",
  "status": 1,
  "type": 1,
  "balance": 2500.00,
  "incomeAmount": 5000.00,
  "limit": 10000.00,
  "addicionalCreditLimit": 5000.00,
  "addicionalCreditLimitStatus": 4,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

### Enums Utilizados

#### StatusClientEnum
- `Active = 1`
- `Inactive = 2`

#### AccountTypeEnum
- `Current = 1`
- `Savings = 2`

#### CreditProposalStatusEnum
- `PreApproved = 1`
- `Approved = 2`
- `Rejected = 3`
- `Pending = 4`

## Exemplo de Uso

### Injeção de Dependência
```csharp
// Program.cs
builder.Services.AddTransient<IClientRepository, ClientRepository>();
```

### Uso no Service
```csharp
public class ClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ClientDto>> CreateClientAsync(ClientRequestBody request)
    {
        var clientDto = new ClientDto
        {
            Name = request.Name,
            Email = request.Email,
            // ... outras propriedades
        };

        var createdClient = await _repository.CreateAsync(clientDto);
        return Result<ClientDto>.Success(createdClient);
    }
}
```

## Validações Implementadas

1. **Email único**: Não permite clientes com o mesmo email
2. **Campos obrigatórios**: Nome, email e telefone são obrigatórios
3. **Formato de email**: Deve conter "@"
4. **Telefone**: Mínimo 10 dígitos
5. **ID único**: Gera automaticamente se não fornecido

## Tratamento de Erros

O repositório lança exceções específicas:
- `InvalidOperationException`: Para validações de negócio
- `Exception`: Para erros de I/O e serialização

## Arquivo de Dados

O arquivo `ClientsData.json` é copiado automaticamente para o diretório de saída durante a compilação e contém os dados mockados dos clientes.

## Thread Safety

Todas as operações de escrita (Create, Update, Delete) são protegidas por lock para garantir consistência dos dados em ambientes multi-thread.
