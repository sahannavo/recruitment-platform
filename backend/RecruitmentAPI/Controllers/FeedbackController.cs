using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.DTOs.Interview;
using RecruitmentAPI.Services.Interfaces;
using System.Security.Claims;

namespace RecruitmentAPI.Controllers;

/// <summary>
/// Controller for managing interview feedback
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(
        IFeedbackService feedbackService,
        ILogger<FeedbackController> logger)
    {
        _feedbackService = feedbackService;
        _logger = logger;
    }

    /// <summary>
    /// Submit feedback for an interview
    /// </summary>
    /// <param name="dto">Feedback details</param>
    /// <returns>The submitted feedback</returns>
    /// <response code="201">Feedback submitted successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Only hiring managers can submit feedback</response>
    /// <response code="404">Interview not found</response>
    [HttpPost]
    [Authorize(Roles = "HiringManager,Admin")]
    [ProducesResponseType(typeof(FeedbackResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackResponseDto>> SubmitFeedback([FromBody] FeedbackSubmitDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var managerId = GetUserId();
            var feedback = await _feedbackService.SubmitFeedbackAsync(dto, managerId);

            _logger.LogInformation("Feedback submitted: {FeedbackId} by manager {ManagerId}", 
                feedback.FeedbackId, managerId);

            return CreatedAtAction(
                nameof(GetFeedbackById),
                new { id = feedback.FeedbackId },
                feedback);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while submitting feedback");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access while submitting feedback");
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title = "Forbidden",
                Detail = ex.Message,
                Status = StatusCodes.Status403Forbidden
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while submitting feedback.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get feedback by ID
    /// </summary>
    /// <param name="id">Feedback ID</param>
    /// <returns>The feedback details</returns>
    /// <response code="200">Returns the feedback</response>
    /// <response code="404">Feedback not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FeedbackResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FeedbackResponseDto>> GetFeedbackById(int id)
    {
        try
        {
            var feedback = await _feedbackService.GetByIdAsync(id);

            if (feedback == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Feedback Not Found",
                    Detail = $"Feedback with ID {id} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback {FeedbackId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving feedback.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get all feedback for a specific interview
    /// </summary>
    /// <param name="interviewId">Interview ID</param>
    /// <returns>List of feedback for the interview</returns>
    /// <response code="200">Returns the list of feedback</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("interview/{interviewId}")]
    [ProducesResponseType(typeof(IEnumerable<FeedbackResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<FeedbackResponseDto>>> GetFeedbackByInterview(int interviewId)
    {
        try
        {
            var feedbacks = await _feedbackService.GetByInterviewAsync(interviewId);

            _logger.LogInformation("Retrieved {Count} feedback entries for interview {InterviewId}", 
                feedbacks.Count(), interviewId);

            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback for interview {InterviewId}", interviewId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving feedback.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get all feedback submitted by the current hiring manager
    /// </summary>
    /// <returns>List of feedback submitted by the manager</returns>
    /// <response code="200">Returns the list of feedback</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Only hiring managers can access this endpoint</response>
    [HttpGet("manager")]
    [Authorize(Roles = "HiringManager,Admin")]
    [ProducesResponseType(typeof(IEnumerable<FeedbackResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<FeedbackResponseDto>>> GetMyFeedback()
    {
        try
        {
            var managerId = GetUserId();
            var feedbacks = await _feedbackService.GetByManagerAsync(managerId);

            _logger.LogInformation("Retrieved {Count} feedback entries for manager {ManagerId}", 
                feedbacks.Count(), managerId);

            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback for manager");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving feedback.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update existing feedback
    /// </summary>
    /// <param name="id">Feedback ID</param>
    /// <param name="dto">Updated feedback details</param>
    /// <returns>Updated feedback</returns>
    /// <response code="200">Feedback updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Can only update your own feedback</response>
    /// <response code="404">Feedback not found</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "HiringManager,Admin")]
    [ProducesResponseType(typeof(FeedbackResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeedbackResponseDto>> UpdateFeedback(
        int id,
        [FromBody] FeedbackSubmitDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var managerId = GetUserId();
            var feedback = await _feedbackService.UpdateFeedbackAsync(id, dto, managerId);

            _logger.LogInformation("Feedback {FeedbackId} updated by manager {ManagerId}", id, managerId);

            return Ok(feedback);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating feedback");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access while updating feedback");
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title = "Forbidden",
                Detail = ex.Message,
                Status = StatusCodes.Status403Forbidden
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feedback {FeedbackId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while updating feedback.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get feedback by decision type
    /// </summary>
    /// <param name="decision">Decision type (Selected, Rejected, Pending)</param>
    /// <returns>List of feedback with the specified decision</returns>
    /// <response code="200">Returns the list of feedback</response>
    /// <response code="400">Invalid decision type</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("decision/{decision}")]
    [Authorize(Roles = "Recruiter,HiringManager,Admin")]
    [ProducesResponseType(typeof(IEnumerable<FeedbackResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<FeedbackResponseDto>>> GetFeedbackByDecision(string decision)
    {
        try
        {
            var feedbacks = await _feedbackService.GetByDecisionAsync(decision);

            _logger.LogInformation("Retrieved {Count} feedback entries with decision {Decision}", 
                feedbacks.Count(), decision);

            return Ok(feedbacks);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid decision type: {Decision}", decision);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Decision",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback by decision {Decision}", decision);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving feedback.",
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

    #endregion
}
