using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Services.Interfaces;
using RecruitmentAPI.Extensions;
using RecruitmentAPI.Services.AI;
using RecruitmentAPI.Repository.Interfaces;

namespace RecruitmentAPI.Controllers
{
    /// <summary>
    /// Controller for candidate operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        private readonly ILogger<CandidatesController> _logger;
        private readonly IAIService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public CandidatesController(
            ICandidateService candidateService,
            ILogger<CandidatesController> logger,
            IAIService aiService,
            IUnitOfWork unitOfWork)
        {
            _candidateService = candidateService;
            _logger = logger;
            _aiService = aiService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get candidate profile
        /// </summary>
        /// <returns>Candidate profile</returns>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(CandidateProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CandidateProfileDto>> GetProfile()
        {
            try
            {
                var userId = User.GetUserId();
                var profile = await _candidateService.GetProfileAsync(userId);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Candidate profile not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting candidate profile");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the profile" });
            }
        }

        /// <summary>
        /// Update candidate profile
        /// </summary>
        /// <param name="updateDto">Profile update data</param>
        /// <returns>Updated profile</returns>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(CandidateProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CandidateProfileDto>> UpdateProfile([FromBody] CandidateUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                var profile = await _candidateService.UpdateProfileAsync(userId, updateDto);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Candidate not found for update");
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid update data");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate profile");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the profile" });
            }
        }

        /// <summary>
        /// Upload CV/document for candidate
        /// </summary>
        /// <param name="file">CV file</param>
        /// <param name="documentType">Type of document (default: CV)</param>
        /// <returns>Uploaded document details</returns>
        [HttpPost("upload-cv")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        public async Task<ActionResult<DocumentResponseDto>> UploadCV(
            [FromForm] IFormFile file,
            [FromQuery] string documentType = "CV")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File is required" });

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    return StatusCode(StatusCodes.Status413PayloadTooLarge,
                        new { message = "File size exceeds 5MB limit" });

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".txt", ".rtf" };
                var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = $"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}" });

                var userId = User.GetUserId();
                var result = await _candidateService.UploadCVAsync(userId, file, documentType);

                return CreatedAtAction(nameof(GetDocuments), new { userId = result.DocumentId }, result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Candidate not found for CV upload");
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid file upload");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading CV");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while uploading the CV" });
            }
        }

        /// <summary>
        /// Parse CV without saving to database, used for auto-fill.
        /// </summary>
        [HttpPost("parse-cv")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RecruitmentAPI.DTOs.AI.ResumeParseResult>> ParseCV([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File is required" });

                var text = await RecruitmentAPI.Helpers.FileTextExtractor.ExtractTextAsync(file);
                
                if (string.IsNullOrWhiteSpace(text))
                    return BadRequest(new { message = "Could not extract any text from the file. If this is a PDF, it might be an image-based scan. Please upload a text-based PDF or a .txt file." });

                var parsedData = await _aiService.ParseResumeAsync(text);
                return Ok(parsedData);
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CV");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while parsing the CV" });
            }
        }

        /// <summary>
        /// Get all documents for the current candidate
        /// </summary>
        /// <returns>List of documents</returns>
        [HttpGet("documents")]
        [ProducesResponseType(typeof(IEnumerable<DocumentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<DocumentResponseDto>>> GetDocuments()
        {
            try
            {
                var userId = User.GetUserId();
                var documents = await _candidateService.GetDocumentsAsync(userId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving documents" });
            }
        }

        /// <summary>
        /// Delete a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>No content</returns>
        [HttpDelete("documents/{documentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteDocument(int documentId)
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _candidateService.DeleteDocumentAsync(userId, documentId);

                if (!result)
                    return NotFound(new { message = "Document not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the document" });
            }
        }
        /// <summary>
        /// Download a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>File stream</returns>
        [HttpGet("documents/{documentId}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadDocument(int documentId, [FromServices] RecruitmentAPI.Services.Interfaces.IBlobStorageService blobStorageService)
        {
            try
            {
                var userId = User.GetUserId();
                var documents = await _candidateService.GetDocumentsAsync(userId);
                var doc = documents.FirstOrDefault(d => d.DocumentId == documentId);
                
                if (doc == null)
                    return NotFound(new { message = "Document not found" });

                var fileStream = await blobStorageService.DownloadFileAsync(doc.BlobUrl);
                
                return File(fileStream, doc.FileType ?? "application/pdf", doc.FileName ?? "resume");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document {DocumentId}", documentId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while downloading the document" });
            }
        }

        /// <summary>
        /// Get parsed skills from candidate's CV
        /// </summary>
        /// <returns>List of parsed skills</returns>
        [HttpGet("skills/parsed")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<string>>> GetParsedSkills()
        {
            try
            {
                var userId = User.GetUserId();
                var skills = await _candidateService.GetParsedSkillsAsync(userId);
                return Ok(skills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parsed skills");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving parsed skills" });
            }
        }

        /// <summary>
        /// Get candidate summary for dashboard
        /// </summary>
        /// <returns>Candidate summary</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(CandidateSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CandidateSummaryDto>> GetSummary()
        {
            try
            {
                var userId = User.GetUserId();
                var summary = await _candidateService.GetCandidateSummaryAsync(userId);
                return Ok(summary);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Candidate summary not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting candidate summary");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving candidate summary" });
            }
        }

        /// <summary>
        /// Get AI-generated summary of the candidate's profile based on their skills and biography
        /// </summary>
        /// <param name="candidateId">The candidate ID</param>
        /// <returns>AI generated text summary</returns>
        [HttpGet("{candidateId}/ai-summary")]
        [Authorize(Roles = "Admin,Recruiter,HiringManager,Candidate")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> GetCandidateAiSummary(int candidateId)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
                if (candidate == null)
                {
                    candidate = await _unitOfWork.Candidates.GetByUserIdAsync(candidateId);
                }
                if (candidate == null)
                    return NotFound(new { message = "Candidate not found." });

                // We don't have a distinct Biography field in the database, 
                // but SkillsSummary stores the concatenated skills and profile text from the frontend.
                var summary = await _aiService.GenerateCandidateProfileSummaryAsync(candidate.SkillsSummary, candidate.Biography);
                
                return Ok(new { summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI summary for candidate {CandidateId}", candidateId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while generating the AI summary" });
            }
        }

        /// <summary>
        /// Get candidate application history
        /// </summary>
        /// <returns>List of applications</returns>
        [HttpGet("applications")]
        [ProducesResponseType(typeof(IEnumerable<ApplicationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetApplicationHistory()
        {
            try
            {
                var userId = User.GetUserId();
                var applications = await _candidateService.GetApplicationHistoryAsync(userId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application history");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving application history" });
            }
        }

        /// <summary>
        /// Update candidate skills
        /// </summary>
        /// <param name="skills">List of skills</param>
        /// <returns>Updated skills summary</returns>
        [HttpPut("skills")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> UpdateSkills([FromBody] List<string> skills)
        {
            try
            {
                if (skills == null || skills.Count == 0)
                    return BadRequest(new { message = "Skills list is required" });

                var userId = User.GetUserId();
                var result = await _candidateService.UpdateSkillsAsync(userId, skills);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Candidate not found for skills update");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skills");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating skills" });
            }
        }

        /// <summary>
        /// Get candidate statistics
        /// </summary>
        /// <returns>Candidate statistics</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(CandidateStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CandidateStatisticsDto>> GetStatistics()
        {
            try
            {
                var userId = User.GetUserId();
                var stats = await _candidateService.GetStatisticsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting candidate statistics");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving statistics" });
            }
        }

        /// <summary>
        /// Deactivate candidate account
        /// </summary>
        /// <returns>No content</returns>
        [HttpPost("deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeactivateAccount()
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _candidateService.DeactivateAccountAsync(userId);

                if (!result)
                    return NotFound(new { message = "Candidate not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating account");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deactivating the account" });
            }
        }

        /// <summary>
        /// Reactivate candidate account
        /// </summary>
        /// <returns>No content</returns>
        [HttpPost("reactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ReactivateAccount()
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _candidateService.ReactivateAccountAsync(userId);

                if (!result)
                    return NotFound(new { message = "Candidate not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating account");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while reactivating the account" });
            }
        }

        /// <summary>
        /// Get job recommendations for the current candidate
        /// </summary>
        /// <param name="limit">Number of recommendations (default: 10)</param>
        /// <returns>List of recommended jobs</returns>
        [HttpGet("recommendations")]
        [ProducesResponseType(typeof(IEnumerable<JobResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetRecommendations([FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.GetUserId();
                var jobs = await _candidateService.GetJobRecommendationsAsync(userId, limit);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job recommendations");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving recommendations" });
            }
        }

        /// <summary>
        /// Get candidate availability status
        /// </summary>
        /// <returns>Availability status</returns>
        [HttpGet("availability")]
        [ProducesResponseType(typeof(AvailabilityStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AvailabilityStatusDto>> GetAvailability()
        {
            try
            {
                var userId = User.GetUserId();
                var availability = await _candidateService.GetAvailabilityStatusAsync(userId);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting availability status");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving availability" });
            }
        }

        /// <summary>
        /// Update candidate availability
        /// </summary>
        /// <param name="availabilityDto">Availability data</param>
        /// <returns>Updated availability</returns>
        [HttpPut("availability")]
        [ProducesResponseType(typeof(AvailabilityStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AvailabilityStatusDto>> UpdateAvailability([FromBody] AvailabilityUpdateDto availabilityDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.GetUserId();
                var availability = await _candidateService.UpdateAvailabilityAsync(userId, availabilityDto);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating availability");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating availability" });
            }
        }
    }
}