using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Load secrets from configuration
var configuration = builder.Configuration;
var azureOpenAIKey = configuration["OpenAISecrets:ApiKey"];
var azureOpenAIEndpoint = "https://sand-and-stones-open-ai-0001.openai.azure.com/";
var azureOpenAIDeploymentName = "gpt-4o-mini";

builder.Services.AddAzureOpenAIChatCompletion(
                        deploymentName: azureOpenAIDeploymentName,
                        endpoint: azureOpenAIEndpoint,
                        apiKey: azureOpenAIKey);

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseCors();

app.MapHub<ChatHub>("/chatHub");

app.Run();