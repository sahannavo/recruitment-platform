using System.Threading.Tasks;

namespace RecruitmentAPI.Services.Interfaces;

public interface IChatbotService
{
    Task<string> AskQuestionAsync(int userId, string role, string userPrompt);
}
