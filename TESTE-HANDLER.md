# Teste do Sistema de Handlers

## Problemas Identificados e Corrigidos

### 1. Namespace do CreditProposalDto
- **Problema**: Estava no namespace `CreditProposalService.Domain.DTOs`
- **Solução**: Movido para `SharedLib.Domain.DTOs`

### 2. Registro de Handlers
- **Problema**: Handler não estava registrado corretamente
- **Solução**: Registrado após o EventBus no Program.cs

### 3. Serialização JSON
- **Problema**: Falta de construtores padrão para serialização
- **Solução**: Adicionados construtores padrão nas classes

### 4. Logging Detalhado
- **Problema**: Logging insuficiente para debug
- **Solução**: Adicionado logging detalhado em todas as etapas

## Como Testar

### 1. Verificar RabbitMQ
```powershell
# Execute o script de teste
powershell -ExecutionPolicy Bypass -File test-rabbitmq.ps1
```

### 2. Iniciar os Serviços

**Terminal 1 - API:**
```bash
cd API
dotnet run
```

**Terminal 2 - CreditProposalService:**
```bash
cd CreditProposalService
dotnet run
```

### 3. Teste 1 - Evento Direto
```bash
# Testar publicação direta de evento
curl -X POST "https://localhost:7001/api/client/test-event" \
  -H "Content-Type: application/json"
```

**Logs esperados na API:**
```
Publishing test event: CreditProposalResultEvent
Declaring queue for publishing: CreditProposalResultEvent
Serialized message: {...}
Successfully published event CreditProposalResultEvent
=== Processing event: CreditProposalResultEvent ===
Creating handler instance for type: CreditProposalHandler
Successfully deserialized event: CreditProposalResultEvent
Invoking Handle method on handler: CreditProposalHandler
Processing CreditProposalResultEvent for client {ClientId}
Client {ClientId} not found for credit proposal update
Successfully processed event CreditProposalResultEvent
```

### 4. Teste 2 - Fluxo Completo
```bash
# Criar um cliente que dispara o fluxo completo
curl -X POST "https://localhost:7001/api/client" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Teste Handler",
    "email": "teste.handler@email.com",
    "phoneNumber": "11987654321",
    "type": 2,
    "balance": 800.00,
    "incomeAmount": 800.00
  }'
```

**Logs esperados:**

**API:**
```
Cliente criado com sucesso.
Publishing event: CreditProposalEvent
CreditProposalEvent published for client {ClientId}
```

**CreditProposalService:**
```
Processing credit proposal for client {ClientId} with income balance {IncomeBalance}
Credit proposal REJECTED for client {ClientId}. Income {IncomeBalance} < Threshold {Threshold}
CreditProposalResultEvent published for client {ClientId}
```

**API (Handler):**
```
=== Processing event: CreditProposalResultEvent ===
Creating handler instance for type: CreditProposalHandler
Successfully deserialized event: CreditProposalResultEvent
Invoking Handle method on handler: CreditProposalHandler
Processing CreditProposalResultEvent for client {ClientId}
Updating client {ClientId} with credit proposal status {Status} and limit {Limit}
Successfully updated client {ClientId} credit proposal
```

### 5. Verificar Resultado
```bash
# Buscar o cliente criado (substitua {CLIENT_ID} pelo ID retornado)
curl -X GET "https://localhost:7001/api/client/{CLIENT_ID}"
```

**Verificar se os campos foram atualizados:**
- `AddicionalCreditLimitStatus`: Deve ser `3` (Rejected)
- `AddicionalCreditLimit`: Deve ser `0`
- `UpdatedAt`: Deve ter sido atualizado

## Troubleshooting

### 1. RabbitMQ não conecta
```bash
# Instalar RabbitMQ via Chocolatey
choco install rabbitmq

# Ou baixar de https://www.rabbitmq.com/download.html
# Iniciar serviço
Start-Service RabbitMQ
```

### 2. Handler não é chamado
- Verificar se o `CreditProposalHandler` está registrado
- Verificar logs do `RabbitMqClientBus` para ver se o evento está sendo recebido
- Verificar se há erros de serialização

### 3. Erro de namespace
- Verificar se todos os usings estão corretos
- Verificar se `CreditProposalDto` está no namespace correto

### 4. Erro de dependência
- Verificar se `IClientRepository` está registrado
- Verificar se `ILogger` está disponível

## Logs de Debug

O sistema agora tem logging detalhado em todas as etapas:

1. **Publish**: Logs de publicação de eventos
2. **Subscribe**: Logs de registro de handlers
3. **ProcessEvent**: Logs detalhados do processamento
4. **Handler**: Logs de execução dos handlers

## Configurações Importantes

### appsettings.json (CreditProposalService)
```json
{
  "MinThreshold": "1200"
}
```

### Dependências Registradas
```csharp
// Program.cs (API)
builder.Services.AddTransient<IEventHandler<CreditProposalResultEvent>, CreditProposalHandler>();

// Program.cs (CreditProposalService)
builder.Services.AddTransient<IEventHandler<CreditProposalEvent>, CreditProposalWorker>();
```

## Monitoramento

Para monitorar o fluxo em tempo real:
1. Observe os logs de ambos os serviços
2. Use o endpoint `/api/client/test-event` para testes rápidos
3. Verifique a interface web do RabbitMQ em http://localhost:15672
