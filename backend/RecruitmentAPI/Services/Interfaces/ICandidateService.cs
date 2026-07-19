using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.DTOs;

namespace RecruitmentAPI.Services.Interfaces
{
    /// <summary>
    /// Service interface for candidate operations
    /// </summary>
    public interface ICandidateService
    {
        /// <summary>
        /// Get candidate profile by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Candidate profile DTO</returns>
        Task<CandidateProfileDto> GetProfileAsync(int userId);

        /// <summary>
        /// Update candidate profile
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="updateDto">Profile update data</param>
        /// <returns>Updated candidate profile</returns>
        Task<CandidateProfileDto> UpdateProfileAsync(int userId, CandidateUpdateDto updateDto);

        /// <summary>
        /// Upload CV/document for a candidate
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="file">CV file</param>
        /// <param name="documentType">Type of document</param>
        /// <returns>Uploaded document details</returns>
        Task<DocumentResponseDto> UploadCVAsync(int userId, IFormFile file, string documentType = "CV");

        /// <summary>
        /// Get parsed skills from candidate's CV
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of parsed skills</returns>
        Task<List<string>> GetParsedSkillsAsync(int userId);

        /// <summary>
        /// Get all documents for a candidate
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of documents</returns>
        Task<IEnumerable<DocumentResponseDto>> GetDocumentsAsync(int userId);

        /// <summary>
        /// Delete a document
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="documentId">Document ID</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteDocumentAsync(int userId, int documentId);

        /// <summary>
        /// Get candidate by email
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>Candidate profile DTO</returns>
        Task<CandidateProfileDto> GetByEmailAsync(string email);

        /// <summary>
        /// Get candidate summary for dashboard
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Candidate summary DTO</returns>
        Task<CandidateSummaryDto> GetCandidateSummaryAsync(int userId);

        /// <summary>
        /// Check if candidate exists
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if candidate exists</returns>
        Task<bool> CandidateExistsAsync(int userId);

        /// <summary>
        /// Get candidate's application history
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of applications with details</returns>
        Task<IEnumerable<ApplicationResponseDto>> GetApplicationHistoryAsync(int userId);

        /// <summary>
        /// Update candidate's skills summary
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="skills">List of skills</param>
        /// <returns>Updated skills summary</returns>
        Task<string> UpdateSkillsAsync(int userId, List<string> skills);

        /// <summary>
        /// Get candidate statistics
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Candidate statistics</returns>
        Task<CandidateStatisticsDto> GetStatisticsAsync(int userId);

        /// <summary>
        /// Deactivate candidate account
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if deactivated successfully</returns>
        Task<bool> DeactivateAccountAsync(int userId);

        /// <summary>
        /// Reactivate candidate account
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if reactivated successfully</returns>
        Task<bool> ReactivateAccountAsync(int userId);

        /// <summary>
        /// Get candidate's job recommendations
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="limit">Number of recommendations</param>
        /// <returns>List of recommended jobs</returns>
        Task<IEnumerable<JobResponseDto>> GetJobRecommendationsAsync(int userId, int limit = 10);

        /// <summary>
        /// Parse CV and extract information
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Parsed resume result</returns>
        Task<ResumeParseResultDto> ParseResumeAsync(int documentId);

        /// <summary>
        /// Get candidate's skills with proficiency levels
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of skills with levels</returns>
        Task<List<SkillDto>> GetSkillsWithLevelsAsync(int userId);

        /// <summary>
        /// Update candidate's profile picture
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="file">Profile picture file</param>
        /// <returns>Profile picture URL</returns>
        Task<string> UpdateProfilePictureAsync(int userId, IFormFile file);

        /// <summary>
        /// Get candidate's availability status
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Availability status</returns>
        Task<AvailabilityStatusDto> GetAvailabilityStatusAsync(int userId);

        /// <summary>
        /// Update candidate's availability
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="availabilityDto">Availability data</param>
        /// <returns>Updated availability</returns>
        Task<AvailabilityStatusDto> UpdateAvailabilityAsync(int userId, AvailabilityUpdateDto availabilityDto);
    }

    /// <summary>
    /// DTO for document response
    /// </summary>
    public class DocumentResponseDto
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; }
        public string BlobUrl { get; set; }
        public string FileType { get; set; }
        public string DocumentType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsParsed { get; set; }
    }

    /// <summary>
    /// DTO for candidate statistics
    /// </summary>
    public class CandidateStatisticsDto
    {
        public int TotalApplications { get; set; }
        public int ActiveApplications { get; set; }
        public int InterviewsScheduled { get; set; }
        public int InterviewsCompleted { get; set; }
        public int OffersReceived { get; set; }
        public double AverageAIScore { get; set; }
        public double HighestAIScore { get; set; }
        public DateTime? LastApplicationDate { get; set; }
        public Dictionary<string, int> ApplicationsByStatus { get; set; }
    }

    /// <summary>
    /// DTO for resume parse result
    /// </summary>
    public class ResumeParseResultDto
    {
        public string ExtractedText { get; set; }
        public List<string> Skills { get; set; }
        public string Experience { get; set; }
        public string Education { get; set; }
        public List<string> Certifications { get; set; }
        public List<string> Languages { get; set; }
        public Dictionary<string, string> ContactInfo { get; set; }
        public List<WorkExperienceDto> WorkExperience { get; set; }
    }

    /// <summary>
    /// DTO for work experience
    /// </summary>
    public class WorkExperienceDto
    {
        public string Company { get; set; }
        public string Title { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public bool IsCurrent { get; set; }
    }

    /// <summary>
    /// DTO for skill with proficiency
    /// </summary>
    public class SkillDto
    {
        public string Name { get; set; }
        public string Proficiency { get; set; } // Beginner, Intermediate, Advanced, Expert
        public int YearsOfExperience { get; set; }
    }

    /// <summary>
    /// DTO for availability status
    /// </summary>
    public class AvailabilityStatusDto
    {
        public bool IsAvailable { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public string NoticePeriod { get; set; }
        public bool IsOpenToOpportunities { get; set; }
        public string PreferredLocations { get; set; }
        public bool WillingToRelocate { get; set; }
        public bool WillingToWorkRemote { get; set; }
    }

    /// <summary>
    /// DTO for availability update
    /// </summary>
    public class AvailabilityUpdateDto
    {
        public bool IsAvailable { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public string NoticePeriod { get; set; }
        public bool IsOpenToOpportunities { get; set; }
        public string PreferredLocations { get; set; }
        public bool WillingToRelocate { get; set; }
        public bool WillingToWorkRemote { get; set; }
    }
}