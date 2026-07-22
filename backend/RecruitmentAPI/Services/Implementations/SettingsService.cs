using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Data;
using RecruitmentAPI.DTOs;
using RecruitmentAPI.Models;
using RecruitmentAPI.Services.Interfaces;

namespace RecruitmentAPI.Services.Implementations;

public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _context;

    public SettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformSettingsDto> GetSettingsAsync()
    {
        var settings = await _context.PlatformSettings.FirstOrDefaultAsync(s => s.Id == 1);
        
        if (settings == null)
        {
            // Fallback in case settings are missing for some reason
            settings = new PlatformSettings { Id = 1 };
            _context.PlatformSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return MapToDto(settings);
    }

    public async Task<PublicPlatformSettingsDto> GetPublicSettingsAsync()
    {
        var settings = await _context.PlatformSettings.FirstOrDefaultAsync(s => s.Id == 1);
        if (settings == null)
        {
            return new PublicPlatformSettingsDto { CompanyName = "Acme Corporation", WebsiteUrl = "https://acme.inc" };
        }
        return new PublicPlatformSettingsDto
        {
            CompanyName = settings.CompanyName,
            WebsiteUrl = settings.WebsiteUrl
        };
    }

    public async Task<PlatformSettingsDto> UpdateSettingsAsync(PlatformSettingsDto settingsDto)
    {
        var settings = await _context.PlatformSettings.FirstOrDefaultAsync(s => s.Id == 1);
        
        if (settings == null)
        {
            settings = new PlatformSettings { Id = 1 };
            _context.PlatformSettings.Add(settings);
        }

        settings.CompanyName = settingsDto.CompanyName;
        settings.Industry = settingsDto.Industry;
        settings.WebsiteUrl = settingsDto.WebsiteUrl;
        settings.OpenAIKey = settingsDto.OpenAIKey;
        settings.AWSKey = settingsDto.AWSKey;
        settings.SendGridApiKey = settingsDto.SendGridApiKey;
        settings.EmailTemplate = settingsDto.EmailTemplate;
        settings.Creativity = settingsDto.Creativity;
        settings.Precision = settingsDto.Precision;
        settings.Penalty = settingsDto.Penalty;
        settings.SystemAlerts = settingsDto.SystemAlerts;
        settings.WeeklyReport = settingsDto.WeeklyReport;
        settings.ApiWarnings = settingsDto.ApiWarnings;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(settings);
    }

    private PlatformSettingsDto MapToDto(PlatformSettings settings)
    {
        return new PlatformSettingsDto
        {
            CompanyName = settings.CompanyName,
            Industry = settings.Industry,
            WebsiteUrl = settings.WebsiteUrl,
            OpenAIKey = settings.OpenAIKey,
            AWSKey = settings.AWSKey,
            SendGridApiKey = settings.SendGridApiKey,
            EmailTemplate = settings.EmailTemplate,
            Creativity = settings.Creativity,
            Precision = settings.Precision,
            Penalty = settings.Penalty,
            SystemAlerts = settings.SystemAlerts,
            WeeklyReport = settings.WeeklyReport,
            ApiWarnings = settings.ApiWarnings
        };
    }
}
