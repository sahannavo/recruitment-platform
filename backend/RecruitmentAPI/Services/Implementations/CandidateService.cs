using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecruitmentAPI.Data;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Helpers;
using RecruitmentAPI.Models;
using RecruitmentAPI.Repositories.Interfaces;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Implementations
{
    /// <summary>
    /// Service implementation for candidate operations
    /// </summary>
    public class CandidateService : ICandidateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IAIService _aiService;
        private readonly ILogger<CandidateService> _logger;

        public CandidateService(
            IUnitOfWork unitOfWork,
            IBlobStorageService blobStorageService,
            IAIService aiService,
            ILogger<CandidateService> logger)
        {
            _unitOfWork = unitOfWork;
            _blobStorageService = blobStorageService;
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// Get candidate profile by user ID
        /// </summary>
        public async Task<CandidateProfileDto> GetProfileAsync(int userId)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {userId} not found");

                var applications = await _unitOfWork.Applications.GetByCandidateAsync(userId);
                var applicationsList = applications.ToList();

                return new CandidateProfileDto
                {
                    UserId = candidate.UserId,
                    FirstName = candidate.FirstName,
                    LastName = candidate.LastName,
                    Email = candidate.Email,
                    Phone = candidate.Phone,
                    Location = candidate.Location,
                    LinkedIn = candidate.LinkedIn,
                    SkillsSummary = candidate.SkillsSummary,
                    IsActive = candidate.IsActive,
                    CreatedAt = candidate.CreatedAt,
                    UpdatedAt = candidate.UpdatedAt,
                    TotalApplications = applicationsList.Count,
                    ActiveApplications = applicationsList.Count(a => a.Status != ApplicationStatus.Rejected
                                                                      && a.Status != ApplicationStatus.Withdrawn)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Update candidate profile
        /// </summary>
        public async Task<CandidateProfileDto> UpdateProfileAsync(int userId, CandidateUpdateDto updateDto)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {userId} not found");

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
                    candidate.FirstName = updateDto.FirstName;

                if (!string.IsNullOrWhiteSpace(updateDto.LastName))
                    candidate.LastName = updateDto.LastName;

                if (!string.IsNullOrWhiteSpace(updateDto.Phone))
                    candidate.Phone = updateDto.Phone;

                if (!string.IsNullOrWhiteSpace(updateDto.Location))
                    candidate.Location = updateDto.Location;

                if (!string.IsNullOrWhiteSpace(updateDto.LinkedIn))
                    candidate.LinkedIn = updateDto.LinkedIn;

                if (!string.IsNullOrWhiteSpace(updateDto.SkillsSummary))
                    candidate.SkillsSummary = updateDto.SkillsSummary;

                candidate.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Profile updated for user {UserId}", userId);
                return await GetProfileAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Upload CV/document for a candidate
        /// </summary>
        public async Task<DocumentResponseDto> UploadCVAsync(int userId, IFormFile file, string documentType = "CV")
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {userId} not found");

                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is required");

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".txt", ".rtf" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    throw new ArgumentException($"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    throw new ArgumentException("File size exceeds 5MB limit");

                // Upload to blob storage
                var blobUrl = await _blobStorageService.UploadFileAsync(file, $"candidates/{userId}/cv");

                var document = new Document
                {
                    CandidateId = userId,
                    FileName = file.FileName,
                    BlobUrl = blobUrl,
                    FileType = file.ContentType,
                    UploadedAt = DateTime.UtcNow,
                    DocumentType = documentType,
                    FileSize = file.Length,
                    FileExtension = extension,
                    IsActive = true,
                    IsParsed = false,
                    DocumentName = file.FileName
                };

                await _unitOfWork.Documents.AddAsync(document);
                await _unitOfWork.SaveChangesAsync();

                // Trigger AI parsing asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ParseResumeAsync(document.DocumentId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing resume for document {DocumentId}", document.DocumentId);
                    }
                });

                _logger.LogInformation("CV uploaded for user {UserId}", userId);

                return new DocumentResponseDto
                {
                    DocumentId = document.DocumentId,
                    FileName = document.FileName,
                    BlobUrl = document.BlobUrl,
                    FileType = document.FileType,
                    DocumentType = document.DocumentType,
                    FileSize = document.FileSize,
                    UploadedAt = document.UploadedAt,
                    IsParsed = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading CV for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get parsed skills from candidate's CV
        /// </summary>
        public async Task<List<string>> GetParsedSkillsAsync(int userId)
        {
            try
            {
                var documents = await _unitOfWork.Documents.GetByCandidateAsync(userId);
                var cvDocument = documents.FirstOrDefault(d => d.DocumentType == "CV" && d.IsParsed);

                if (cvDocument == null)
                    return new List<string>();

                var parseResult = await _aiService.ParseResumeAsync(cvDocument.BlobUrl);
                return parseResult.Skills ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parsed skills for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Parse CV and extract information
        /// </summary>
        public async Task<ResumeParseResultDto> ParseResumeAsync(int documentId)
        {
            try
            {
                var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
                if (document == null)
                    throw new KeyNotFoundException($"Document with ID {documentId} not found");

                var parseResult = await _aiService.ParseResumeAsync(document.BlobUrl);

                // Update document
                document.IsParsed = true;
                document.ParsedAt = DateTime.UtcNow;
                document.ParseResult = System.Text.Json.JsonSerializer.Serialize(parseResult);

                // Update candidate skills
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(document.CandidateId);
                if (candidate != null && parseResult.Skills != null && parseResult.Skills.Any())
                {
                    candidate.SkillsSummary = string.Join(", ", parseResult.Skills);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Resume parsed for document {DocumentId}", documentId);

                return new ResumeParseResultDto
                {
                    ExtractedText = parseResult.ExtractedText,
                    Skills = parseResult.Skills,
                    Experience = parseResult.Experience,
                    Education = parseResult.Education,
                    Certifications = parseResult.Certifications,
                    Languages = parseResult.Languages,
                    ContactInfo = parseResult.ContactInfo,
                    WorkExperience = parseResult.WorkExperience?.Select(we => new WorkExperienceDto
                    {
                        Company = we.Company,
                        Title = we.Title,
                        StartDate = we.StartDate,
                        EndDate = we.EndDate,
                        Description = we.Description,
                        IsCurrent = we.IsCurrent
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing resume for document {DocumentId}", documentId);
                throw;
            }
        }

        /// <summary>
        /// Get all documents for a candidate
        /// </summary>
        public async Task<IEnumerable<DocumentResponseDto>> GetDocumentsAsync(int userId)
        {
            try
            {
                var documents = await _unitOfWork.Documents.GetByCandidateAsync(userId);
                return documents.Select(d => new DocumentResponseDto
                {
                    DocumentId = d.DocumentId,
                    FileName = d.FileName,
                    BlobUrl = d.BlobUrl,
                    FileType = d.FileType,
                    DocumentType = d.DocumentType,
                    FileSize = d.FileSize,
                    UploadedAt = d.UploadedAt,
                    IsParsed = d.IsParsed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Delete a document
        /// </summary>
        public async Task<bool> DeleteDocumentAsync(int userId, int documentId)
        {
            try
            {
                var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
                if (document == null || document.CandidateId != userId)
                    return false;

                // Delete from blob storage
                await _blobStorageService.DeleteFileAsync(document.BlobUrl);

                // Soft delete
                document.IsActive = false;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Document {DocumentId} deleted for user {UserId}", documentId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId} for user {UserId}", documentId, userId);
                throw;
            }
        }

        /// <summary>
        /// Get candidate by email
        /// </summary>
        public async Task<CandidateProfileDto> GetByEmailAsync(string email)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByEmailAsync(email);
                if (candidate == null)
                    return null;

                return await GetProfileAsync(candidate.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting candidate by email {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Get candidate summary for dashboard
        /// </summary>
        public async Task<CandidateSummaryDto> GetCandidateSummaryAsync(int userId)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {userId} not found");

                var applications = await _unitOfWork.Applications.GetByCandidateAsync(userId);
                var applicationsList = applications.ToList();

                return new CandidateSummaryDto
                {
                    UserId = candidate.UserId,
                    FullName = $"{candidate.FirstName} {candidate.LastName}",
                    Email = candidate.Email,
                    Location = candidate.Location,
                    SkillsSummary = candidate.SkillsSummary,
                    TotalApplications = applicationsList.Count,
                    CreatedAt = candidate.CreatedAt,
                    IsActive = candidate.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting candidate summary for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Check if candidate exists
        /// </summary>
        public async Task<bool> CandidateExistsAsync(int userId)
        {
            try
            {
                return await _unitOfWork.Candidates.GetByUserIdAsync(userId) != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking candidate existence for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get candidate's application history
        /// </summary>
        public async Task<IEnumerable<ApplicationResponseDto>> GetApplicationHistoryAsync(int userId)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetByCandidateAsync(userId);
                return applications.Select(a => new ApplicationResponseDto
                {
                    ApplicationId = a.ApplicationId,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    Company = a.Job.Department,
                    Status = a.Status.ToString(),
                    AppliedAt = a.AppliedAt,
                    AI_Score = a.AI_Score,
                    CandidateId = a.CandidateId,
                    CandidateName = $"{a.Candidate.FirstName} {a.Candidate.LastName}",
                    CandidateEmail = a.Candidate.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application history for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Update candidate's skills summary
        /// </summary>
        public async Task<string> UpdateSkillsAsync(int userId, List<string> skills)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {userId} not found");

                candidate.SkillsSummary = string.Join(", ", skills);
                candidate.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Skills updated for user {UserId}", userId);
                return candidate.SkillsSummary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skills for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get candidate statistics
        /// </summary>
        public async Task<CandidateStatisticsDto> GetStatisticsAsync(int userId)
        {
            try
            {
                var applications = await _unitOfWork.Applications.GetByCandidateAsync(userId);
                var applicationsList = applications.ToList();

                var stats = new CandidateStatisticsDto
                {
                    TotalApplications = applicationsList.Count,
                    ActiveApplications = applicationsList.Count(a => a.Status != ApplicationStatus.Rejected
                                                                     && a.Status != ApplicationStatus.Withdrawn),
                    InterviewsScheduled = applicationsList.Count(a => a.Status == ApplicationStatus.InterviewScheduled),
                    InterviewsCompleted = applicationsList.Count(a => a.Status == ApplicationStatus.Interviewed),
                    OffersReceived = applicationsList.Count(a => a.Status == ApplicationStatus.Hired),
                    LastApplicationDate = applicationsList.Any() ? applicationsList.Max(a => a.AppliedAt) : null,
                    ApplicationsByStatus = applicationsList
                        .GroupBy(a => a.Status.ToString())
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                if (applicationsList.Any(a => a.AI_Score.HasValue))
                {
                    var scores = applicationsList.Where(a => a.AI_Score.HasValue).Select(a => a.AI_Score.Value);
                    stats.AverageAIScore = scores.Average();
                    stats.HighestAIScore = scores.Max();
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Deactivate candidate account
        /// </summary>
        public async Task<bool> DeactivateAccountAsync(int userId)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    return false;

                candidate.IsActive = false;
                candidate.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Account deactivated for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating account for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Reactivate candidate account
        /// </summary>
        public async Task<bool> ReactivateAccountAsync(int userId)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    return false;

                candidate.IsActive = true;
                candidate.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Account reactivated for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating account for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get candidate's job recommendations
        /// </summary>
        public async Task<IEnumerable<JobResponseDto>> GetJobRecommendationsAsync(int userId, int limit = 10)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {userId} not found");

                var skills = candidate.SkillsSummary?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string>();
                var jobs = await _unitOfWork.Jobs.GetRecommendedJobsForCandidateAsync(skills, limit);

                return jobs.Select(j => new JobResponseDto
                {
                    JobId = j.JobId,
                    Title = j.Title,
                    Description = j.Description,
                    Requirements = j.Requirements,
                    Department = j.Department,
                    Location = j.Location,
                    SalaryRange = j.SalaryRange,
                    Status = j.Status.ToString(),
                    PostedBy = j.PostedBy,
                    CreatedAt = j.CreatedAt,
                    UpdatedAt = j.UpdatedAt,
                    ApplicantsCount = j.Applications?.Count ?? 0,
                    IsRemote = j.IsRemote,
                    EmploymentType = j.EmploymentType,
                    ExperienceLevel = j.ExperienceLevel
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job recommendations for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get candidate's skills with proficiency levels
        /// </summary>
        public async Task<List<SkillDto>> GetSkillsWithLevelsAsync(int userId)
        {
            // This would require a skills tracking table
            // Placeholder implementation
            return new List<SkillDto>();
        }

        /// <summary>
        /// Update candidate's profile picture
        /// </summary>
        public async Task<string> UpdateProfilePictureAsync(int userId, IFormFile file)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {userId} not found");

                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is required");

                var blobUrl = await _blobStorageService.UploadFileAsync(file, $"candidates/{userId}/profile");

                candidate.ProfilePictureUrl = blobUrl;
                candidate.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Profile picture updated for user {UserId}", userId);
                return blobUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile picture for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get candidate's availability status
        /// </summary>
        public async Task<AvailabilityStatusDto> GetAvailabilityStatusAsync(int userId)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {userId} not found");

                return new AvailabilityStatusDto
                {
                    IsAvailable = candidate.IsAvailable,
                    AvailableFrom = candidate.AvailableFrom,
                    NoticePeriod = candidate.NoticePeriod,
                    IsOpenToOpportunities = candidate.IsOpenToOpportunities,
                    PreferredLocations = candidate.PreferredLocations,
                    WillingToRelocate = candidate.WillingToRelocate,
                    WillingToWorkRemote = candidate.WillingToWorkRemote
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting availability status for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Update candidate's availability
        /// </summary>
        public async Task<AvailabilityStatusDto> UpdateAvailabilityAsync(int userId, AvailabilityUpdateDto availabilityDto)
        {
            try
            {
                var candidate = await _unitOfWork.Candidates.GetByUserIdAsync(userId);
                if (candidate == null)
                    throw new KeyNotFoundException($"Candidate with ID {userId} not found");

                candidate.IsAvailable = availabilityDto.IsAvailable;
                candidate.AvailableFrom = availabilityDto.AvailableFrom;
                candidate.NoticePeriod = availabilityDto.NoticePeriod;
                candidate.IsOpenToOpportunities = availabilityDto.IsOpenToOpportunities;
                candidate.PreferredLocations = availabilityDto.PreferredLocations;
                candidate.WillingToRelocate = availabilityDto.WillingToRelocate;
                candidate.WillingToWorkRemote = availabilityDto.WillingToWorkRemote;
                candidate.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Availability updated for user {UserId}", userId);
                return await GetAvailabilityStatusAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating availability for user {UserId}", userId);
                throw;
            }
        }
    }
}