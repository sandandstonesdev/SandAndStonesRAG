using Azure;
using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json.Serialization;

public class BlobToCosmosFunction
{
    private readonly ILogger _logger;
    private readonly OpenAIClient _openAIClient;
    private readonly string _embeddingDeployment;

    public BlobToCosmosFunction(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<BlobToCosmosFunction>();
        var endpoint = configuration["AzureOpenAI:Endpoint"];
        var key = configuration["AzureOpenAI:Key"];
        _embeddingDeployment = configuration["AzureOpenAI:EmbeddingDeployment"] ?? string.Empty;

        ArgumentException.ThrowIfNullOrWhiteSpace(_embeddingDeployment, "AzureOpenAI:EmbeddingDeployment");
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint, "AzureOpenAI:Endpoint");
        ArgumentException.ThrowIfNullOrWhiteSpace(key, "AzureOpenAI:Key");

        _openAIClient = new OpenAIClient(
            new Uri(endpoint),
            new AzureKeyCredential(key));
    }

    [Function("BlobToCosmosFunction")]
    [CosmosDBOutput(
        databaseName: "document-embeddings",
        containerName: "embeddings",
        Connection = "CosmosDBConnection")]
    public async Task<Document> Run(
        [BlobTrigger("documents/{name}", Connection = "AzureWebJobsStorage")] byte[] blobContent,
        string name)
    {
        _logger.LogInformation($"Blob trigger function processed blob\n Name:{name} \n Size: {blobContent.Length} Bytes");
        string content = Encoding.UTF8.GetString(blobContent);

        var embeddingOptions = new EmbeddingsOptions(_embeddingDeployment, [content]);
        var embeddingResponse = await _openAIClient.GetEmbeddingsAsync(
            embeddingOptions,
            cancellationToken: default
        );

        var embedding = embeddingResponse.Value.Data[0].Embedding.ToArray();

        var document = new Document
        {
            Id = Guid.NewGuid().ToString(),
            BlobName = name,
            ContentLength = blobContent.Length,
            ContentPreview = Encoding.UTF8.GetString(blobContent, 0, Math.Min(blobContent.Length, 100)),
            Embedding = embedding
        };

        return document;
    }

    public class Document
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public string BlobName { get; set; }

        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset Updated { get; set; } = DateTimeOffset.Now;
        public int ContentLength { get; set; }
        public string ContentPreview { get; set; }
        public float[] Embedding { get; set; }
    }
}