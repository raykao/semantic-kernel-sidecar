using Newtonsoft.Json;
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
    // private static readonly KernelBuilder _builder = new KernelBuilder();

    private readonly IKernel _kernel;

    // private readonly List<Dictionary<string, ISKFunction>> _semanticSkillsList = new List<Dictionary<string, ISKFunction>>();
    private Dictionary<string, Dictionary<string, ISKFunction>> _semanticSkillsList = new Dictionary<string, Dictionary<string, ISKFunction>>();
    
    // This is the constructor for the SemanticSkillsController - it should auto load the skills from the /SemanticSkills directory which is mounted to the container
    public SemanticSkillsController(ILogger<SemanticSkillsController> logger, IKernel kernel)
    {
        _logger = logger;

        _kernel = kernel;

        var planner = new SequentialPlanner(_kernel);

        var skillsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "SemanticSkills");

        // var skillsDirectory = Path.Combine(currentDirectory, "SemanticSkills");

        string[] subdirectories = Directory.GetDirectories(skillsDirectory);
            
        foreach (string subdirectoryPath in subdirectories)
        {
            string skillName = Path.GetFileName(subdirectoryPath);

            var skill = _kernel.ImportSemanticSkillFromDirectory(skillsDirectory, skillName);

            if (skill is Dictionary<string, ISKFunction>)
            {
                _semanticSkillsList[skillName] = (Dictionary<string, ISKFunction>)skill;
            }
            else
            {
                Console.WriteLine($"skipping {skillName} from {skillsDirectory} because it is not a SemanticSkill");
            }
        }
    }

    // this shoudl return all the skills in the /SemanticSkills directory and any that have been created via the CreateNewSkill method
    [HttpGet(Name = "GetSemanticSkills")]
    public async Task<IActionResult> GetSemanticSkills()
    {
        // return a list of all the skills in _semanticSkillsList as a json object
        return Ok(_semanticSkillsList);
    }

    [HttpPost("invoke/{skillName}/{subSkillName}", Name = "Invoke")]
    public async Task<IActionResult> InvokeSkill(string skillName, string subSkillName, [FromBody] SemanticAsk semanticAsk)
    {
        Console.WriteLine($"Executing Skill: {skillName}");

        var skill = _semanticSkillsList[skillName];

        try {
            var result = await skill[subSkillName].InvokeAsync(semanticAsk.request);
            var responseObj = new { response = result.Result.Replace("\n", "") };

            return Ok(responseObj);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return StatusCode(500, $"An error occurred: {ex}");
        }
    }
}


// // // TODO: Implement Invoke method
// // // This should be able to call the specific skill and subskill

// try {
//     var skill = _semanticSkillsList.Find(s => s.ContainsKey(skillName));
//     if (skill == null)
//     {
//         return NotFound($"Skill '{skillName}' not found");
//     }

//     var subskill = skill[skillName];

//     if (subskill == null)
//     {
//         return NotFound($"Subskill '{subSkillName}' not found for skill '{skillName}'");
//     }

//     var userQuestion = requestBody.userQuestion.ToString();

//     var result = await subskill.InvokeAsync(userQuestion);

//     return Ok(result);
// }
// catch (Exception ex) 
// {
//     return StatusCode(500, $"An error occurred: {ex.Message}");
// }


//     [HttpPost(Name = "CreateSkill")]
//     public IActionResult CreateSkill()
//     {
//         // TODO: Implement createnewskill method
//         // This should allow the developer to post a JSON object to create a new skill over http similar to creating an inline skill
//         // This should also persist the skill to disk (i.e. save to the /SemanticSkills directory)
//         return Ok();

//     }
