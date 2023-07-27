using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Sequential;
using Microsoft.SemanticKernel.SkillDefinition;
using semantic_kernel_sidecar.models;


namespace semantic_kernel_sidecar.Controllers;

[ApiController]
[Route("[controller]")]
public class SemanticSkillsController : ControllerBase
{
    private readonly ILogger<SemanticSkillsController> _logger;
    private static readonly KernelBuilder _builder = new KernelBuilder();
    private readonly List<Dictionary<string, ISKFunction>> _semanticSkillsList = new List<Dictionary<string, ISKFunction>>();
    
    // This is the constructor for the SemanticSkillsController - it should auto load the skills from the /SemanticSkills directory which is mounted to the container
    public SemanticSkillsController(ILogger<SemanticSkillsController> logger)
    {
        _logger = logger;

        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var settingsFilePath = Path.Combine(baseDirectory, "settings.json");

        // Load the settings from the file
        var settingsJson = File.ReadAllText(settingsFilePath);

        var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = JsonSerializer.Deserialize<(bool, string, string, string, string)>(settingsJson);

        // var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

        if (useAzureOpenAI)
            _builder.WithAzureTextCompletionService(model, azureEndpoint, apiKey);
        else
            _builder.WithOpenAITextCompletionService(model, apiKey, orgId);

        var _kernel = _builder.Build();

        // Load native skill into the _kernel registry, sharing its functions with prompt templates
        var planner = new SequentialPlanner(_kernel);

        var skillsDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var directoryPath = Path.Combine(skillsDirectory, "/SemanticSkills");

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

    // this shoudl return all the skills in the /SemanticSkills directory and any that have been created via the CreateNewSkill method
    [HttpGet(Name = "GetSemanticSkills")]
    public IEnumerable<Dictionary<string, ISKFunction>> GetSemanticSkills()
    {
        return _semanticSkillsList;
    }

    [HttpPost("{skillName}/{subSkillName}", Name = "Invoke")]
    public async Task<IActionResult> InvokeSkill(string skillName, string subSkillName, [FromBody] SemanticSkill requestBody)
    {
    
        // // TODO: Implement Invoke method
        // // This should be able to call the specific skill and subskill
        
        try {
            var skill = _semanticSkillsList.Find(s => s.ContainsKey(skillName));
            if (skill == null)
            {
                return NotFound($"Skill '{skillName}' not found");
            }

            var subskill = skill[skillName];

            if (subskill == null)
            {
                return NotFound($"Subskill '{subSkillName}' not found for skill '{skillName}'");
            }

            var userQuestion = requestBody.userQuestion.ToString();

            var result = await subskill.InvokeAsync(userQuestion);

            return Ok(result);
        }
        catch (Exception ex) 
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpPost(Name = "CreateSkill")]
    public IActionResult CreateSkill()
    {
        // TODO: Implement createnewskill method
        // This should allow the developer to post a JSON object to create a new skill over http similar to creating an inline skill
        // This should also persist the skill to disk (i.e. save to the /SemanticSkills directory)
        return Ok();

    }
}
