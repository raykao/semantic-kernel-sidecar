using Microsoft.SemanticKernel;
using System.Text.Json;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the required services
builder.Services.AddSingleton<IKernel>(provider =>
{
    var builder = new KernelBuilder();
    var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = SKSettings.LoadFromFile("settings.json");

    if (useAzureOpenAI)
        builder.WithAzureTextCompletionService(model, azureEndpoint, apiKey);
    else
        builder.WithOpenAITextCompletionService(model, apiKey, orgId);

    IKernel kernel = builder.Build();

    return kernel;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
