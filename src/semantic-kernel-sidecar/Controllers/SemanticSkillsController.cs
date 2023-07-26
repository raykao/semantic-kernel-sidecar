using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Sequential;

namespace semantic_kernel_sidecar.Controllers;

[ApiController]
[Route("[controller]")]
public class SemanticSkillsController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<SemanticSkillsController> _logger;


    private static readonly KernelBuilder _builder = new KernelBuilder();
    private readonly List<SemanticSkill> _semanticSkillsList = new List<Dictionary<string, ISKFunction>>();

    
    public SemanticSkillsController(ILogger<SemanticSkills> logger)
    {
        _logger = logger;

        string directoryPath = "/SemanticSkills";

        var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

        if (useAzureOpenAI)
            _builder.WithAzureTextCompletionService(model, azureEndpoint, apiKey);
        else
            _builder.WithOpenAITextCompletionService(model, apiKey, orgId);

        var _kernel = _builder.Build();

        // Load native skill into the _kernel registry, sharing its functions with prompt templates
        var planner = new SequentialPlanner(_kernel);

        string[] subdirectories = Directory.GetDirectories(directoryPath);
            
        foreach (string subdirectoryPath in subdirectories)
        {
            string skillName = Path.GetFileName(subdirectoryPath);
            var skill = _kernel.ImportSemanticSkillFromDirectory(directoryPath, skillName);
            if (skill is Dictionary<string, ISKFunction>)
            {
                _semanticSkillsList.Add((Dictionary<string, ISKFunction>)skill);
                Console.WriteLine($"loaded {skillName} from {directoryPath}");
            }
            else
            {
                Console.WriteLine($"skipping {skillName} from {directoryPath} because it is not a SemanticSkill");
            }
        }
    }

    [HttpGet(Name = "GetSemanticSkills")]
    public IEnumerable<SemanticSkill> Get()
    {
        return _semanticSkillsList;
    }

    [HttpPost(Name = "Invoke")]
    public IEnumerable<SemanticSkill> Get()
    {
        // TODO: Implement Invoke method
        return null;

    }
}
