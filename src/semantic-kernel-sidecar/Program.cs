using Microsoft.SemanticKernel;
using System.Text.Json;

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

    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    var settingsFilePath = Path.Combine(baseDirectory, "settings.json");

    // Load the settings from the file
    var settingsJson = File.ReadAllText(settingsFilePath);

    var (model, apiKey, orgId) = JsonSerializer.Deserialize<(string, string, string)>(settingsJson);

    // var (model, apiKey, orgId) = Settings.LoadFromFile();

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
