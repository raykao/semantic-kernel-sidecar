// #r "nuget: Microsoft.SemanticKernel, 0.17.230626.1-preview"

// #!import config/Settings.cs
// #!import config/Utils.cs

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Sequential;

var builder = new KernelBuilder();

// Configure AI backend used by the kernel
var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

if (useAzureOpenAI)
    builder.WithAzureTextCompletionService(model, azureEndpoint, apiKey);
else
    builder.WithOpenAITextCompletionService(model, apiKey, orgId);

var kernel = builder.Build();

// Load native skill into the kernel registry, sharing its functions with prompt templates
var planner = new SequentialPlanner(kernel);

string directoryPath = "/skills";

string[] subdirectories = Directory.GetDirectories(directoryPath);

foreach (string subdirectoryPath in subdirectories)
{
    string skillName = Path.GetFileName(subdirectoryPath);
    kernel.ImportSemanticSkillFromDirectory(directoryPath, skillName);
    Console.WriteLine($"loaded {skillName} from {directoryPath}");
}