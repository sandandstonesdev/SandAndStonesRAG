using ConversationService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

public interface IChatHubClient
{
    Task ReceiveMessage(string user, string message);
}

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class ChatHub : Hub<IChatHubClient>
{
    private readonly SummarizeService _summarizeService;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ChatHistory _chatHistory = [];

    public ChatHub(
        IChatCompletionService chatCompletionService,
        SummarizeService summarizeService
        )
    {
        _summarizeService = summarizeService;
        _chatCompletionService = chatCompletionService;
        _chatHistory.AddSystemMessage("You are a helpful assistant.");
    }

    public async Task SendMessage(string user, string message)
    {
        _chatHistory.AddUserMessage(message);

        // Add Cosmos History Sotre and Summary Functionality there
    
        var response = await _chatCompletionService.GetChatMessageContentAsync(_chatHistory);
        var lastMessage = response.Items.Last().ToString();

        await Clients.All.ReceiveMessage(user, message);
        await Clients.All.ReceiveMessage("Chatbot", lastMessage ?? string.Empty);
    }
}
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
