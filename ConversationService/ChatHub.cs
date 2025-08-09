using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;

public interface IChatHubClient
{
    Task ReceiveMessage(string user, string message);
}

public class ChatHub : Hub<IChatHubClient>
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ChatHistory _chatHistory = new ChatHistory();

    public ChatHub(IChatCompletionService chatCompletionService)
    {
        _chatCompletionService = chatCompletionService;
        _chatHistory.AddSystemMessage("You are a helpful assistant.");
    }

    public async Task SendMessage(string user, string message)
    {
        // Add user message to chat history
        _chatHistory.AddUserMessage(message);

        // Generate chatbot response using Semantic Kernel
        var response = await _chatCompletionService.GetChatMessageContentAsync(_chatHistory);
        var lastMessage = response.Items.Last().ToString(); // Convert KernelContent to string

        await Clients.All.ReceiveMessage(user, message);

        // Broadcast chatbot response to all clients
        await Clients.All.ReceiveMessage("Chatbot", lastMessage ?? string.Empty);
    }
}
