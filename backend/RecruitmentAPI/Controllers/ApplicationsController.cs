using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Models;
using RecruitmentAPI.Services.Interfaces;
using RecruitmentAPI.Extensions;

namespace RecruitmentAPI.Controllers
{
    /// <summary>
    /// Controller for application operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly ILogger<ApplicationsController> _logger;

        public ApplicationsController(
            IApplicationService applicationService,
            ILogger<ApplicationsController> logger)
        {
            _applicationService = applicationService;
            _logger = logger;
        }

        /// <summary>
        /// Submit a new job application
        /// </summary>
        /// <param name="submitDto">Application submission data</param>
        /// <returns>Submitted application</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApplicationResponseDto>> SubmitApplication([FromBody] ApplicationSubmitDto submitDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                submitDto.CandidateId = userId; // Ensure candidate can only apply for themselves

                var result = await _applicationService.SubmitApplicationAsync(submitDto);

                return CreatedAtAction(nameof(GetById), new { id = result.ApplicationId }, result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Candidate or job not found for application submission");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid application submission");
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid application data");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting application");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while submitting the application" });
            }
        }

        /// <summary>
        /// Get application by ID
        /// </summary>
        /// <param name="id">Application ID</param>
        /// <returns>Application details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationResponseDto>> GetById(int id)
        {
            try
            {
                var application = await _applicationService.GetByIdAsync(id);

                // Check authorization
                var userId = User.GetUserId();
                var userRole = User.GetRole();

                if (application.CandidateId != userId && userRole != "Admin" && userRole != "Recruiter" && userRole != "HiringManager")
                    return Forbid();

                return Ok(application);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Application {ApplicationId} not found", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application {ApplicationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the application" });
            }
        }

        /// <summary>
        /// Download the candidate's resume for a specific application
        /// </summary>
        /// <param name="id">Application ID</param>
        /// <returns>File stream</returns>
        [HttpGet("{id}/resume/download")]
        [Authorize(Roles = "Recruiter,Admin,HiringManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadResume(int id, [FromServices] RecruitmentAPI.Services.Interfaces.IBlobStorageService blobStorageService, [FromServices] RecruitmentAPI.Repository.Interfaces.IUnitOfWork unitOfWork)
        {
            try
            {
                var application = await unitOfWork.Applications.GetApplicationWithDetailsAsync(id);
                if (application == null)
                    return NotFound(new { message = "Application not found" });

                var userId = User.GetUserId();
                var userRole = User.GetRole();

                if (application.Job.Recruiter.UserId != userId && userRole != "Admin" && userRole != "HiringManager")
                    return Forbid();

                var cvDoc = application.Candidate.Documents?.FirstOrDefault(d => (d.DocumentType == "CV" || d.DocumentType == "Resume") && d.IsActive);
                if (cvDoc == null || string.IsNullOrEmpty(cvDoc.BlobUrl))
                    return NotFound(new { message = "Resume not found for this candidate" });

                var fileStream = await blobStorageService.DownloadFileAsync(cvDoc.BlobUrl);
                return File(fileStream, cvDoc.FileType ?? "application/pdf", cvDoc.FileName ?? "resume.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading resume for application {ApplicationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while downloading the resume" });
            }
        }

        /// <summary>
        /// Get all applications for the current candidate
        /// </summary>
        /// <returns>List of applications</returns>
        [HttpGet("candidate")]
        [ProducesResponseType(typeof(IEnumerable<ApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetCandidateApplications()
        {
            try
            {
                var userId = User.GetUserId();
                var applications = await _applicationService.GetByCandidateAsync(userId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting candidate applications");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving applications" });
            }
        }

        /// <summary>
        /// Get all applications for a specific job (Recruiter/Admin only)
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>List of applications</returns>
        [HttpGet("recruiter/{jobId}")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(IEnumerable<ApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetApplicationsForJob(int jobId)
        {
            try
            {
                var applications = await _applicationService.GetByJobAsync(jobId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications for job {JobId}", jobId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving applications" });
            }
        }

        /// <summary>
        /// Get all applications for the current recruiter
        /// </summary>
        /// <returns>List of applications</returns>
        [HttpGet("recruiter")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(IEnumerable<ApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetRecruiterApplications()
        {
            try
            {
                var userId = User.GetUserId();
                var applications = await _applicationService.GetByRecruiterAsync(userId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications for recruiter {UserId}", User.GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving applications" });
            }
        }

        /// <summary>
        /// Update application status
        /// </summary>
        /// <param name="id">Application ID</param>
        /// <param name="updateDto">Status update data</param>
        /// <returns>Updated application</returns>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Recruiter,Admin,HiringManager")]
        [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationResponseDto>> UpdateStatus(
            int id,
            [FromBody] ApplicationStatusUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var application = await _applicationService.UpdateStatusAsync(id, updateDto);
                return Ok(application);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Application {ApplicationId} not found for status update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid status for application {ApplicationId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid status transition for application {ApplicationId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating application {ApplicationId} status", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating application status" });
            }
        }

        /// <summary>
        /// Withdraw an application
        /// </summary>
        /// <param name="id">Application ID</param>
        /// <returns>No content</returns>
        [HttpPut("{id}/withdraw")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> WithdrawApplication(int id)
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _applicationService.WithdrawAsync(id, userId);

                if (!result)
                    return NotFound(new { message = $"Application with ID {id} not found" });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized withdrawal attempt for application {ApplicationId}", id);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid withdrawal for application {ApplicationId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing application {ApplicationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while withdrawing the application" });
            }
        }

        /// <summary>
        /// Get applications by status
        /// </summary>
        /// <param name="status">Application status</param>
        /// <returns>List of applications</returns>
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Recruiter,Admin,HiringManager")]
        [ProducesResponseType(typeof(IEnumerable<ApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetByStatus(string status)
        {
            try
            {
                if (!Enum.TryParse<ApplicationStatus>(status, true, out var applicationStatus))
                    return BadRequest(new { message = $"Invalid status: {status}" });

                var applications = await _applicationService.GetByStatusAsync(applicationStatus);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications by status {Status}", status);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving applications" });
            }
        }

        /// <summary>
        /// Get applications with high AI scores
        /// </summary>
        /// <param name="threshold">Minimum AI score threshold (default: 70)</param>
        /// <returns>List of applications</returns>
        [HttpGet("high-score")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(IEnumerable<ApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetWithHighAIScore([FromQuery] double threshold = 70)
        {
            try
            {
                var applications = await _applicationService.GetWithHighAIScoreAsync(threshold);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications with high AI score");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving applications" });
            }
        }

        /// <summary>
        /// Get application statistics
        /// </summary>
        /// <returns>Application statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Recruiter,Admin,HiringManager")]
        [ProducesResponseType(typeof(ApplicationStatistics), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApplicationStatistics>> GetStatistics()
        {
            try
            {
                var stats = await _applicationService.GetApplicationStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application statistics");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// Get applications with full details including interviews and feedback
        /// </summary>
        /// <param name="id">Application ID</param>
        /// <returns>Application with details</returns>
        [HttpGet("{id}/details")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(ApplicationWithInterviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationWithInterviewDto>> GetApplicationWithDetails(int id)
        {
            try
            {
                var application = await _applicationService.GetApplicationWithDetailsAsync(id);
                return Ok(application);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Application {ApplicationId} not found for details", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application details for {ApplicationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving application details" });
            }
        }

        /// <summary>
        /// Get pending review applications
        /// </summary>
        /// <returns>List of applications pending review</returns>
        [HttpGet("pending-review")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(IEnumerable<ApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetPendingReview()
        {
            try
            {
                var applications = await _applicationService.GetPendingReviewApplicationsAsync();
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending review applications");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving applications" });
            }
        }

        /// <summary>
        /// Get applications shortlisted and pending manager review
        /// </summary>
        /// <returns>List of applications pending manager review</returns>
        [HttpGet("manager-review")]
        [Authorize(Roles = "HiringManager,Admin")]
        [ProducesResponseType(typeof(IEnumerable<ApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetManagerReview()
        {
            try
            {
                var applications = await _applicationService.GetManagerReviewApplicationsAsync();
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manager review applications");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving applications" });
            }
        }

        /// <summary>
        /// Search applications by candidate name or job title
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching applications</returns>
        [HttpGet("search")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(IEnumerable<ApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> Search([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest(new { message = "Search term is required" });

                var applications = await _applicationService.SearchApplicationsAsync(searchTerm);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching applications with term {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while searching applications" });
            }
        }

        /// <summary>
        /// Bulk update application status
        /// </summary>
        /// <param name="applicationIds">List of application IDs</param>
        /// <param name="status">New status</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>Number of applications updated</returns>
        [HttpPut("bulk-status")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<int>> BulkUpdateStatus(
            [FromBody] List<int> applicationIds,
            [FromQuery] string status,
            [FromQuery] string notes = null)
        {
            try
            {
                if (applicationIds == null || applicationIds.Count == 0)
                    return BadRequest(new { message = "Application IDs are required" });

                if (!Enum.TryParse<ApplicationStatus>(status, true, out var applicationStatus))
                    return BadRequest(new { message = $"Invalid status: {status}" });

                var count = await _applicationService.BulkUpdateStatusAsync(applicationIds, applicationStatus, notes);
                return Ok(new { updated = count, message = $"{count} applications updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk status update");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating applications" });
            }
        }

        /// <summary>
        /// Recalculate AI scores for all applications of a job
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Number of applications updated</returns>
        [HttpPost("{jobId}/recalculate-ai-scores")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> RecalculateAIScores(int jobId)
        {
            try
            {
                var count = await _applicationService.RecalculateAIScoresForJobAsync(jobId);
                return Ok(new { updated = count, message = $"AI scores recalculated for {count} applications" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Job {JobId} not found for AI score recalculation", jobId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating AI scores for job {JobId}", jobId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while recalculating AI scores" });
            }
        }

        /// <summary>
        /// Get application count by status for a job
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Dictionary of status and count</returns>
        [HttpGet("count-by-status/{jobId}")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Dictionary<string, int>>> GetApplicationCountByStatus(int jobId)
        {
            try
            {
                var counts = await _applicationService.GetApplicationCountByStatusAsync(jobId);
                return Ok(counts.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application count by status for job {JobId}", jobId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving application counts" });
            }
        }

        /// <summary>
        /// Get active applications for the current candidate
        /// </summary>
        /// <returns>List of active applications</returns>
        [HttpGet("candidate/active")]
        [ProducesResponseType(typeof(IEnumerable<ApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetActiveApplications()
        {
            try
            {
                var userId = User.GetUserId();
                var applications = await _applicationService.GetActiveApplicationsForCandidateAsync(userId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active applications");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving active applications" });
            }
        }
    }
}