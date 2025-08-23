# Teste de conectividade do RabbitMQ
Write-Host "Testando conectividade do RabbitMQ..." -ForegroundColor Green

try {
    # Verificar se o RabbitMQ está rodando
    $rabbitmqStatus = Get-Service -Name "RabbitMQ" -ErrorAction SilentlyContinue
    
    if ($rabbitmqStatus -and $rabbitmqStatus.Status -eq "Running") {
        Write-Host "✓ RabbitMQ está rodando" -ForegroundColor Green
    } else {
        Write-Host "✗ RabbitMQ não está rodando ou não foi encontrado" -ForegroundColor Red
        Write-Host "Para instalar o RabbitMQ:" -ForegroundColor Yellow
        Write-Host "1. Baixe de: https://www.rabbitmq.com/download.html" -ForegroundColor Yellow
        Write-Host "2. Ou use Chocolatey: choco install rabbitmq" -ForegroundColor Yellow
        Write-Host "3. Inicie o serviço: Start-Service RabbitMQ" -ForegroundColor Yellow
    }
    
    # Testar conexão TCP
    $tcpTest = Test-NetConnection -ComputerName "localhost" -Port 5672 -InformationLevel Quiet
    
    if ($tcpTest) {
        Write-Host "✓ Porta 5672 está acessível" -ForegroundColor Green
    } else {
        Write-Host "✗ Porta 5672 não está acessível" -ForegroundColor Red
    }
    
    # Testar interface web (porta 15672)
    $webTest = Test-NetConnection -ComputerName "localhost" -Port 15672 -InformationLevel Quiet
    
    if ($webTest) {
        Write-Host "✓ Interface web está acessível (http://localhost:15672)" -ForegroundColor Green
    } else {
        Write-Host "✗ Interface web não está acessível" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "Erro ao testar RabbitMQ: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nPara verificar manualmente:" -ForegroundColor Cyan
Write-Host "1. Abra http://localhost:15672 (usuário: guest, senha: guest)" -ForegroundColor Cyan
Write-Host "2. Verifique se há filas criadas" -ForegroundColor Cyan
Write-Host "3. Verifique se há mensagens sendo processadas" -ForegroundColor Cyan
