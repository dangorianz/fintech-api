using fintech_api.DTOs;
using fintech_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace fintech_api.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly IPaymentService _paymentService;

    public LoansController(ILoanService loanService, IPaymentService paymentService)
    {
        _loanService = loanService;
        _paymentService = paymentService;
    }

    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] SimulateLoanRequest request)
    {
        try
        {
            var result = await _loanService.SimulateAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLoanRequest request)
    {
        try
        {
            var result = await _loanService.CreateAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? userId)
    {
        var result = await _loanService.GetAllAsync(userId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _loanService.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{id}/schedule")]
    public async Task<IActionResult> GetSchedule(Guid id)
    {
        var result = await _loanService.GetScheduleAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/payments")]
    public async Task<IActionResult> PayNextInstallment(Guid id, [FromBody] PayNextInstallmentRequest request)
    {
        try
        {
            var result = await _paymentService.PayNextInstallmentAsync(id, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _loanService.ApproveAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPatch("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await _loanService.RejectAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
