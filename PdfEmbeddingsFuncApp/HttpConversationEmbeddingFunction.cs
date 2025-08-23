using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;
using System.Text.Json;
using System.Text.Json.Serialization;

public class HttpConversationEmbeddingFunction
{
    private readonly ILogger _logger;
    private readonly AzureOpenAIClient _openAIClient;
    private readonly string _embeddingDeployment;

    public HttpConversationEmbeddingFunction(
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        AzureOpenAIClient openAIClient)
    {
        _logger = loggerFactory.CreateLogger<HttpConversationEmbeddingFunction>();
        _embeddingDeployment = configuration["AzureOpenAI:EmbeddingDeployment"] ?? string.Empty;

        ArgumentException.ThrowIfNullOrWhiteSpace(_embeddingDeployment, "AzureOpenAI:EmbeddingDeployment");

        _openAIClient = openAIClient;
    }

    [Function("HttpConversationEmbeddingFunction")]
    [CosmosDBOutput(
        databaseName: "chatbot-embeddings",
        containerName: "conversation-history-embeddings",
        Connection = "CosmosDBConnection")]
    public async Task<ConversationDocument> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var requestBody = await JsonSerializer.DeserializeAsync<ConversationEmbeddingRequest>(req.Body);

        if (string.IsNullOrWhiteSpace(requestBody?.Message))
        {
            throw new ArgumentException("Request body must contain a 'message' property.");
        }

        _logger.LogInformation($"Http trigger function processed conversation message\n User:{requestBody?.UserId} \n Message: {requestBody?.Message}");

        var embeddingClient = _openAIClient.GetEmbeddingClient(_embeddingDeployment);

        var embeddingResponse = await embeddingClient.GenerateEmbeddingsAsync(
            [requestBody?.Message]
        );

        var embeddings = embeddingResponse.Value.First();
        ReadOnlyMemory<float> vector = embeddings.ToFloats();

        var document = new ConversationDocument
        {
            Id = Guid.NewGuid().ToString(),
            UserId = requestBody.UserId,
            Message = requestBody.Message,
            Created = requestBody.Created,
            Updated = requestBody.Created,
            MessageLength = requestBody.Message.Length,
            Embedding = vector.ToArray()
        };

        return document;
    }

    public class ConversationEmbeddingRequest
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("created")]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class ConversationDocument
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("content-length")]
        public int MessageLength { get; set; }

        [JsonPropertyName("created")]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
        [JsonPropertyName("updated")]
        public DateTimeOffset Updated { get; set; } = DateTimeOffset.Now;
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; }
    }
}