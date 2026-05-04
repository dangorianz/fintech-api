using fintech_api.DTOs;
using fintech_api.Models;
using fintech_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace fintech_api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        var result = await _transactionService.CreateAsync(request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] TransactionType? type,
        [FromQuery] TransactionStatus? status)
    {
        var result = await _transactionService.GetAllAsync(type, status);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _transactionService.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}