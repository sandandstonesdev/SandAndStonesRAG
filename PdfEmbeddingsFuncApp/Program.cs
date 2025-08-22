using Azure;
using Azure.AI.OpenAI;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<OpenAIClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["AzureOpenAI:Endpoint"];
    var key = config["AzureOpenAI:Key"];
    ArgumentException.ThrowIfNullOrWhiteSpace(endpoint, "AzureOpenAI:Endpoint");
    ArgumentException.ThrowIfNullOrWhiteSpace(key, "AzureOpenAI:Key");

    return new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
});

// --- Cosmos DB resource creation (clean, isolated) ---
{
    var config = builder.Configuration;
    var cosmosConnectionString = config["CosmosDBConnection"];
    var databaseName = "chatbot-embeddings";

    // Container 1
    var container1 = "conversation-history-embeddings";
    var partitionKey1 = "/userId"; // Adjust as needed

    // Container 2
    var container2 = "document-embeddings";
    var partitionKey2 = "/id"; // Adjust as needed

    using var cosmosClient = new CosmosClient(cosmosConnectionString);

    await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);

    var database = cosmosClient.GetDatabase(databaseName);
    await database.CreateContainerIfNotExistsAsync(new ContainerProperties
    {
        Id = container1,
        PartitionKeyPath = partitionKey1
    });
    await database.CreateContainerIfNotExistsAsync(new ContainerProperties
    {
        Id = container2,
        PartitionKeyPath = partitionKey2
    });
}
// --- End Cosmos DB resource creation ---

builder.Build().Run();
