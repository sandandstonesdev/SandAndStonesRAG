using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Load secrets from configuration
var configuration = builder.Configuration;
var azureOpenAIKey = configuration["OpenAISecrets:ApiKey"];
var azureOpenAIEndpoint = "https://proj-sand-and-stones-ra-resource.openai.azure.com/";
var azureOpenAIDeploymentName = "gpt-4o-mini";

builder.Services.AddAzureOpenAIChatCompletion(
                        deploymentName: azureOpenAIDeploymentName,
                        endpoint: azureOpenAIEndpoint,
                        apiKey: azureOpenAIKey);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins("http://localhost:3001") // Or "*", for testing only
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseCors();

app.MapHub<ChatHub>("/chatHub");

app.Run();