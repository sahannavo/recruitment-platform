using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.DTOs.Interview;
using RecruitmentAPI.Services.Interfaces;
using System.Security.Claims;

namespace RecruitmentAPI.Controllers;

/// <summary>
/// Controller for managing interviews
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _interviewService;
    private readonly ILogger<InterviewsController> _logger;

    public InterviewsController(
        IInterviewService interviewService,
        ILogger<InterviewsController> logger)
    {
        _interviewService = interviewService;
        _logger = logger;
    }

    /// <summary>
    /// Schedule a new interview
    /// </summary>
    /// <param name="dto">Interview scheduling details</param>
    /// <returns>The scheduled interview</returns>
    /// <response code="201">Interview scheduled successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Application not found</response>
    [HttpPost("schedule")]
    [Authorize(Roles = "Recruiter,HiringManager,Admin")]
    [ProducesResponseType(typeof(InterviewResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InterviewResponseDto>> ScheduleInterview([FromBody] ScheduleInterviewDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var interview = await _interviewService.ScheduleAsync(dto, userId);

            _logger.LogInformation("Interview scheduled: {InterviewId} by user {UserId}", interview.InterviewId, userId);

            return CreatedAtAction(
                nameof(GetInterviewById),
                new { id = interview.InterviewId },
                interview);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while scheduling interview");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling interview");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while scheduling the interview.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get interviews for the current user
    /// </summary>
    /// <returns>List of interviews based on user role</returns>
    /// <response code="200">Returns the list of interviews</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<InterviewResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<InterviewResponseDto>>> GetMyInterviews()
    {
        try
        {
            var userId = GetUserId();
            var role = GetUserRole();

            var interviews = await _interviewService.GetByUserAsync(userId, role);

            _logger.LogInformation("Retrieved {Count} interviews for user {UserId} with role {Role}", 
                interviews.Count(), userId, role);

            return Ok(interviews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving interviews for user");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving interviews.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get interview by ID
    /// </summary>
    /// <param name="id">Interview ID</param>
    /// <returns>The interview details</returns>
    /// <response code="200">Returns the interview</response>
    /// <response code="404">Interview not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InterviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InterviewResponseDto>> GetInterviewById(int id)
    {
        try
        {
            var interview = await _interviewService.GetByIdAsync(id);

            if (interview == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Interview Not Found",
                    Detail = $"Interview with ID {id} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(interview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving interview {InterviewId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving the interview.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update interview status
    /// </summary>
    /// <param name="id">Interview ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated interview</returns>
    /// <response code="200">Status updated successfully</response>
    /// <response code="400">Invalid status</response>
    /// <response code="404">Interview not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Recruiter,HiringManager,Admin")]
    [ProducesResponseType(typeof(InterviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InterviewResponseDto>> UpdateInterviewStatus(
        int id,
        [FromBody] UpdateInterviewStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var interview = await _interviewService.UpdateStatusAsync(id, request.Status, userId);

            _logger.LogInformation("Interview {InterviewId} status updated to {Status} by user {UserId}", 
                id, request.Status, userId);

            return Ok(interview);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating interview status");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating interview status for {InterviewId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while updating the interview status.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Cancel an interview
    /// </summary>
    /// <param name="id">Interview ID</param>
    /// <returns>Success status</returns>
    /// <response code="204">Interview cancelled successfully</response>
    /// <response code="404">Interview not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = "Recruiter,HiringManager,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelInterview(int id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _interviewService.CancelAsync(id, userId);

            if (!result)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Interview Not Found",
                    Detail = $"Interview with ID {id} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            _logger.LogInformation("Interview {InterviewId} cancelled by user {UserId}", id, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling interview {InterviewId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while cancelling the interview.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get available time slots for scheduling
    /// </summary>
    /// <param name="date">Date to check availability</param>
    /// <param name="duration">Interview duration in minutes</param>
    /// <returns>List of available time slots</returns>
    /// <response code="200">Returns available time slots</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("availability")]
    [Authorize(Roles = "Recruiter,HiringManager,Admin")]
    [ProducesResponseType(typeof(IEnumerable<DateTime>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailability(
        [FromQuery] DateTime date,
        [FromQuery] int duration = 60)
    {
        try
        {
            var availableSlots = await _interviewService.GetAvailabilityAsync(date, duration);
            return Ok(availableSlots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving availability");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving availability.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    #region Helper Methods

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? "Candidate";
    }

    #endregion
}

/// <summary>
/// Request model for updating interview status
/// </summary>
public class UpdateInterviewStatusRequest
{
    /// <summary>
    /// New status (Scheduled, Completed, Cancelled, Rescheduled)
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
