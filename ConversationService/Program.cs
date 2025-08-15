using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.SemanticKernel;
using ConversationService;

var builder = WebApplication.CreateBuilder(args);

// Load secrets from configuration
var configuration = builder.Configuration;
var azureOpenAIKey = configuration["OpenAISecrets:ApiKey"];
var azureOpenAIEndpoint = "https://proj-sand-and-stones-ra-resource.openai.azure.com/";
var azureOpenAIDeploymentName = "gpt-4o-mini";

builder.Services.Configure<CorsConfig>(builder.Configuration.GetSection("Cors"));

builder.Services.AddAzureOpenAIChatCompletion(
                        deploymentName: azureOpenAIDeploymentName,
                        endpoint: azureOpenAIEndpoint,
                        apiKey: azureOpenAIKey);


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        var corsOptions = configuration.GetSection("Cors").Get<CorsConfig>() ?? new CorsConfig();

        _ = corsOptions.AllowedOrigins.Length > 0 ?
            policyBuilder.WithOrigins(corsOptions.AllowedOrigins) :
            policyBuilder.AllowAnyOrigin();

        _ = corsOptions.AllowedHeaders.Length > 0 && corsOptions.AllowedHeaders[0] != "*" ?
            policyBuilder.WithHeaders(corsOptions.AllowedHeaders) :
            policyBuilder.AllowAnyHeader();

        _ = corsOptions.AllowedMethods.Length > 0 && corsOptions.AllowedMethods[0] != "*" ?
            policyBuilder.WithMethods(corsOptions.AllowedMethods) :
            policyBuilder.AllowAnyMethod();

        _ = corsOptions.AllowCredentials ?
            policyBuilder.AllowCredentials() :
            policyBuilder.DisallowCredentials();
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.MapHub<ChatHub>("/chatHub");

app.Run();