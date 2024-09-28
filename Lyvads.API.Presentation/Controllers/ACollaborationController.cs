using Lyvads.Application.Dtos.SuperAdminDtos;
using Lyvads.Application.Implementations;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ACollaborationController : ControllerBase
{
    private readonly CollaborationService _collaborationService;
    private readonly ILogger<CollaborationController> _logger;

    public ACollaborationController(CollaborationService collaborationService, ILogger<CollaborationController> logger)
    {
        _collaborationService = collaborationService;
        _logger = logger;
    }

    // GET: api/collaboration
    [HttpGet]
    public async Task<ActionResult<ServerResponse<List<CollaborationDto>>>> GetCollaborations()
    {
        _logger.LogInformation("Handling request to fetch all collaborations.");
        var response = await _collaborationService.GetCollaborationsAsync();

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Failed to fetch collaborations: {ResponseMessage}", response.ResponseMessage);
            return NotFound(response);
        }

        return Ok(response);
    }

    // GET: api/collaboration/{id}
    [HttpGet("{collaborationId}")]
    public async Task<ActionResult<ServerResponse<CollaborationDto>>> GetCollaborationDetails(string collaborationId)
    {
        _logger.LogInformation("Handling request to fetch collaboration details for ID: {CollaborationId}", collaborationId);
        var response = await _collaborationService.GetCollaborationDetailsAsync(collaborationId);

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Failed to fetch collaboration details: {ResponseMessage}", response.ResponseMessage);
            return NotFound(response);
        }

        return Ok(response);
    }

    // GET: api/collaboration/download-receipt/{id}
    [HttpGet("download-receipt/{collaborationId}")]
    public async Task<ActionResult<ServerResponse<FileStreamResult>>> DownloadReceipt(string collaborationId)
    {
        _logger.LogInformation("Handling request to download receipt for collaboration ID: {CollaborationId}", collaborationId);
        var response = await _collaborationService.DownloadReceiptAsync(collaborationId);

        if (!response.IsSuccessful)
        {
            _logger.LogWarning("Failed to download receipt: {ResponseMessage}", response.ResponseMessage);
            return NotFound(response);
        }

        return File(response.Data.FileStream, "application/pdf", response.Data.FileDownloadName);
    }
}
