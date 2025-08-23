using Microsoft.Azure.Cosmos;
using ConversationService.Models;
using Microsoft.Azure.Cosmos.Linq;

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