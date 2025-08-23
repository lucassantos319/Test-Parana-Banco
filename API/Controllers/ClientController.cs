using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedLib.Domain.DTOs;
using SharedLib.Domain.Requests;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;
        private readonly IConfiguration _configuration; 
        private readonly IClientService _clientService;

        public ClientController(
            IClientService clientService,
            ILogger<ClientController> logger, 
            IConfiguration configuration)
        {
            _clientService = clientService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClientRequestBody requestBody)
        {
            var result = await _clientService.CreateClientAsync(requestBody);
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to create client: {Message}", result.Message);
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _clientService.GetAllClientsAsync();
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to get clients: {Message}", result.Message);
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _clientService.GetClientByIdAsync(id);
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to get client: {Message}", result.Message);
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ClientRequestBody requestBody)
        {
            var result = await _clientService.UpdateClientAsync(id, requestBody);
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to update client: {Message}", result.Message);
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _clientService.DeleteClientAsync(id);
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to delete client: {Message}", result.Message);
                return BadRequest(result);
            }

            return Ok(result);
        }

    }
}
