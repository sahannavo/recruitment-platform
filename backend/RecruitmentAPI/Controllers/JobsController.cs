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
    /// Controller for job operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly ILogger<JobsController> _logger;

        public JobsController(
            IJobService jobService,
            ILogger<JobsController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        /// <summary>
        /// Get all job postings with pagination and filters
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="department">Filter by department</param>
        /// <param name="location">Filter by location</param>
        /// <param name="status">Filter by status</param>
        /// <param name="employmentType">Filter by employment type</param>
        /// <param name="experienceLevel">Filter by experience level</param>
        /// <param name="isRemote">Filter by remote</param>
        /// <param name="searchTerm">Search term</param>
        /// <returns>Paginated job list</returns>
        [HttpGet]
        [ProducesResponseType(typeof(JobListResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<JobListResponseDto>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string department = null,
            [FromQuery] string location = null,
            [FromQuery] string status = null,
            [FromQuery] string employmentType = null,
            [FromQuery] string experienceLevel = null,
            [FromQuery] bool? isRemote = null,
            [FromQuery] string searchTerm = null)
        {
            try
            {
                var filters = new JobFilterDto
                {
                    Department = department,
                    Location = location,
                    Status = status,
                    EmploymentType = employmentType,
                    ExperienceLevel = experienceLevel,
                    IsRemote = isRemote,
                    SearchTerm = searchTerm
                };

                var result = await _jobService.GetAllAsync(pageNumber, pageSize, filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all jobs");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving jobs" });
            }
        }

        /// <summary>
        /// Get job posting by ID
        /// </summary>
        /// <param name="id">Job ID</param>
        /// <returns>Job details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JobResponseDto>> GetById(int id)
        {
            try
            {
                var job = await _jobService.GetByIdAsync(id);
                return Ok(job);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Job {JobId} not found", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job {JobId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the job" });
            }
        }

        /// <summary>
        /// Create a new job posting
        /// </summary>
        /// <param name="jobPostDto">Job creation data</param>
        /// <returns>Created job</returns>
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<JobResponseDto>> Create([FromBody] JobPostDto jobPostDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var recruiterId = User.GetUserId();
                var job = await _jobService.CreateAsync(jobPostDto, recruiterId);

                return CreatedAtAction(nameof(GetById), new { id = job.JobId }, job);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recruiter not found for job creation");
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid job creation data");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the job" });
            }
        }

        /// <summary>
        /// Update a job posting
        /// </summary>
        /// <param name="id">Job ID</param>
        /// <param name="jobUpdateDto">Job update data</param>
        /// <returns>Updated job</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JobResponseDto>> Update(int id, [FromBody] JobUpdateDto jobUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var job = await _jobService.UpdateAsync(id, jobUpdateDto);
                return Ok(job);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Job {JobId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid job update data for {JobId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job {JobId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the job" });
            }
        }

        /// <summary>
        /// Delete a job posting (soft delete - archive)
        /// </summary>
        /// <param name="id">Job ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _jobService.DeleteAsync(id);

                if (!result)
                    return NotFound(new { message = $"Job with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job {JobId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the job" });
            }
        }

        /// <summary>
        /// Get recommended jobs for the current candidate
        /// </summary>
        /// <param name="limit">Number of recommendations (default: 10)</param>
        /// <returns>List of recommended jobs</returns>
        [HttpGet("recommended")]
        [ProducesResponseType(typeof(IEnumerable<JobResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetRecommendedJobs([FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.GetUserId();
                var jobs = await _jobService.GetRecommendedJobsAsync(userId, limit);
                return Ok(jobs);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Candidate not found for recommendations");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended jobs");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving recommendations" });
            }
        }

        /// <summary>
        /// Get active job postings
        /// </summary>
        /// <returns>List of active jobs</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<JobResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetActiveJobs()
        {
            try
            {
                var jobs = await _jobService.GetActiveJobsAsync();
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active jobs");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving active jobs" });
            }
        }

        /// <summary>
        /// Get jobs by department
        /// </summary>
        /// <param name="department">Department name</param>
        /// <returns>List of jobs in the department</returns>
        [HttpGet("department/{department}")]
        [ProducesResponseType(typeof(IEnumerable<JobResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetByDepartment(string department)
        {
            try
            {
                var jobs = await _jobService.GetByDepartmentAsync(department);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by department {Department}", department);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving jobs" });
            }
        }

        /// <summary>
        /// Get jobs by recruiter
        /// </summary>
        /// <param name="recruiterId">Recruiter ID</param>
        /// <returns>List of jobs posted by the recruiter</returns>
        [HttpGet("recruiter/{recruiterId}")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(IEnumerable<JobResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetByRecruiter(int recruiterId)
        {
            try
            {
                var userId = User.GetUserId();
                var userRole = User.GetRole();

                // Recruiters can only see their own jobs
                if (userRole != "Admin" && userId != recruiterId)
                    return Forbid();

                var jobs = await _jobService.GetByRecruiterAsync(recruiterId);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by recruiter {RecruiterId}", recruiterId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving jobs" });
            }
        }

        /// <summary>
        /// Search jobs by keyword
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching jobs</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<JobResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> Search([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest(new { message = "Search term is required" });

                var jobs = await _jobService.SearchJobsAsync(searchTerm);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching jobs with term {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while searching jobs" });
            }
        }

        /// <summary>
        /// Update job status
        /// </summary>
        /// <param name="id">Job ID</param>
        /// <param name="status">New status</param>
        /// <returns>Updated job</returns>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JobResponseDto>> UpdateStatus(int id, [FromQuery] string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                    return BadRequest(new { message = "Status is required" });

                var job = await _jobService.UpdateStatusAsync(id, status);
                return Ok(job);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Job {JobId} not found for status update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid status for job {JobId}: {Status}", id, status);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job {JobId} status", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating job status" });
            }
        }

        /// <summary>
        /// Get job statistics
        /// </summary>
        /// <returns>Job statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(JobStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<JobStatisticsDto>> GetStatistics()
        {
            try
            {
                var stats = await _jobService.GetJobStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job statistics");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving job statistics" });
            }
        }

        /// <summary>
        /// Get jobs by location
        /// </summary>
        /// <param name="location">Location</param>
        /// <returns>List of jobs in the location</returns>
        [HttpGet("location/{location}")]
        [ProducesResponseType(typeof(IEnumerable<JobResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetByLocation(string location)
        {
            try
            {
                var jobs = await _jobService.GetByLocationAsync(location);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by location {Location}", location);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving jobs" });
            }
        }

        /// <summary>
        /// Get jobs by employment type
        /// </summary>
        /// <param name="employmentType">Employment type</param>
        /// <returns>List of jobs with the employment type</returns>
        [HttpGet("employment-type/{employmentType}")]
        [ProducesResponseType(typeof(IEnumerable<JobResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetByEmploymentType(string employmentType)
        {
            try
            {
                var jobs = await _jobService.GetByEmploymentTypeAsync(employmentType);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by employment type {EmploymentType}", employmentType);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving jobs" });
            }
        }

        /// <summary>
        /// Get jobs by experience level
        /// </summary>
        /// <param name="experienceLevel">Experience level</param>
        /// <returns>List of jobs with the experience level</returns>
        [HttpGet("experience-level/{experienceLevel}")]
        [ProducesResponseType(typeof(IEnumerable<JobResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetByExperienceLevel(string experienceLevel)
        {
            try
            {
                var jobs = await _jobService.GetByExperienceLevelAsync(experienceLevel);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by experience level {ExperienceLevel}", experienceLevel);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving jobs" });
            }
        }

        /// <summary>
        /// Clone a job posting
        /// </summary>
        /// <param name="id">Job ID to clone</param>
        /// <returns>Cloned job</returns>
        [HttpPost("{id}/clone")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JobResponseDto>> CloneJob(int id)
        {
            try
            {
                var recruiterId = User.GetUserId();
                var job = await _jobService.CloneJobAsync(id, recruiterId);

                return CreatedAtAction(nameof(GetById), new { id = job.JobId }, job);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Job {JobId} not found for cloning", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning job {JobId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while cloning the job" });
            }
        }

        /// <summary>
        /// Get job with application statistics
        /// </summary>
        /// <param name="id">Job ID</param>
        /// <returns>Job with statistics</returns>
        [HttpGet("{id}/stats")]
        [Authorize(Roles = "Recruiter,Admin")]
        [ProducesResponseType(typeof(JobWithStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JobWithStatsDto>> GetJobWithStats(int id)
        {
            try
            {
                var job = await _jobService.GetJobWithStatsAsync(id);
                return Ok(job);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Job {JobId} not found for stats", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job stats for {JobId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving job statistics" });
            }
        }
    }
}