using System.Threading.Tasks;
using RecruitmentAPI.DTOs;

namespace RecruitmentAPI.Services.Interfaces;

public interface ISettingsService
{
    Task<PlatformSettingsDto> GetSettingsAsync();
    Task<PlatformSettingsDto> UpdateSettingsAsync(PlatformSettingsDto settingsDto);
    Task<PublicPlatformSettingsDto> GetPublicSettingsAsync();
}
