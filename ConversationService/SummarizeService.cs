using ConversationService.Models;

namespace ConversationService;

public class SummarizeService
{
    public SummarizeService(IConfiguration config)
    {
    }

    public async Task AddSummaryAsync(Summary summary)
    {
        
    }

    public async Task<List<Summary>> GetSummariesAsync(string userId)
    {
        return default;
    }
}