using AccessRequestHub.Api.Exceptions;
using AccessRequestHub.Api.DTOs;
using AccessRequestHub.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestHub.Api.Controllers;

[ApiController]
[Route("api/requests")]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;

    public RequestsController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    private string? GetActorEmail() =>
        Request.Headers.TryGetValue("x-user-email", out var value)
            ? value.ToString()
            : null;

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto dto)
    {
        var actorEmail = GetActorEmail();
        if (string.IsNullOrWhiteSpace(actorEmail))
            return Unauthorized(new
            {
                success = false,
                status = 401,
                error = "Missing or empty x-user-email header."
            });

        try
        {
            var (response, wasCreated) = await _requestService.CreateRequestAsync(actorEmail, dto);

            if (wasCreated)
            {
                return CreatedAtAction(nameof(GetRequest), new
                {
                    success = true,
                    status = 201,
                    id = response.Id
                }, response);
            }
            else
            {
                return Ok(response);
            }
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new
            {   
                success = false,
                status = 400,
                error = ex.Message
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new
            {   
                success = false,
                status = 404,
                error = ex.Message
            });
        }
    }
    [HttpGet]
    public async Task<IActionResult> ListRequests()
    {
        var actorEmail = GetActorEmail();
        if (string.IsNullOrWhiteSpace(actorEmail))
            return Unauthorized(new
            {   
                success = false,
                status = 401,
                error = "Missing or empty x-user-email header."
            });

        try
        {
            var requests = await _requestService.ListRequestsAsync(actorEmail);
            return Ok(requests);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new
            {   
                success = false,
                status = 404,
                error = ex.Message
            });
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new
            {
                success = false,
                status = 403,
                error = ex.Message
            });
        }
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRequest(Guid id)
    {
        var actorEmail = GetActorEmail();
        if (string.IsNullOrWhiteSpace(actorEmail))
            return Unauthorized(new
            {   
                success = false,
                status = 401,
                error = "Missing or empty x-user-email header."
            });

        try
        {
            var detail = await _requestService.GetRequestDetailAsync(actorEmail, id);
            return detail is null
                ? NotFound(new
                {   
                    success = false,
                    status = 404,
                    error = $"Request '{id}' not found."
                }) : Ok(detail);
        }
        catch (NotFoundException ex) 
        { 
            return NotFound(new 
            { 
                success = false, 
                status = 404, 
                error = ex.Message 
            }); }
        catch (ForbiddenException ex) 
        { 
            return StatusCode(403, new 
            { 
                success = false, 
                status = 403, 
                error = ex.Message 
            }); }
    }
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveRequest(Guid id, [FromBody] ApproveRequestDto dto)
    {
        var actorEmail = GetActorEmail();
        if (string.IsNullOrWhiteSpace(actorEmail))
            return Unauthorized(new
            {   
                success = false,
                status = 401,
                error = "Missing or empty x-user-email header."
            });

        try
        {
            var response = await _requestService.ApproveRequestAsync(actorEmail, id, dto);
            return Ok(response);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new 
            { 
                success = false, 
                status = 400, 
                error = ex.Message 
            });
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new 
            { 
                success = false, 
                status = 403, 
                error = ex.Message 
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new 
            { 
                success = false, 
                status = 404, 
                error = ex.Message 
            });
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new 
            { 
                success = false, 
                status = 409, 
                error = ex.Message 
            });
        }
    }
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectRequest(Guid id, [FromBody] RejectRequestDto dto)
    {
        var actorEmail = GetActorEmail();
        if (string.IsNullOrWhiteSpace(actorEmail))
            return Unauthorized(new
            {
                success = false,
                status = 401,
                error = "Missing or empty x-user-email header."
            });

        try
        {
            var response = await _requestService.RejectRequestAsync(actorEmail, id, dto);
            return Ok(response);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new 
            { 
                success = false, 
                status = 400, 
                error = ex.Message 
            });
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new 
            { 
                success = false, 
                status = 403, 
                error = ex.Message 
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new 
            { 
                success = false, 
                status = 404, 
                error = ex.Message 
            });
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new 
            { 
                success = false, 
                status = 409, 
                error = ex.Message 
            });
        }
    }
}
