using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ms_agent_framework;
using OpenAI.Chat;

namespace ToolUsage;

public class ToolUsage : ISubApplication
{
  [Description("Get the weather for a given location.")]
  static string GetWeather([Description("The location to get the weather for.")] string location)
      => $"The weather in {location} is cloudy with a high of 15°C.";

  private readonly ILogger<ToolUsage> _logger;
  private readonly AIAgent agent;

  public string AppName { get; private set; }

  public string AppDescription { get; private set; }

  public ToolUsage(ILogger<ToolUsage> logger)
  {
    _logger = logger;

    var endpoint = AgentConfig.AzureEndpoint;
    var deploymentName = AgentConfig.AzureDeploymentName ?? "gpt-4o-mini";
    // expose identity for Program discovery
    this.AppName = nameof(ToolUsage);
    this.AppDescription = "Demo agent showing how to use tools with Azure OpenAI via MS Agent Framework SDK.";
    if (!string.IsNullOrEmpty(endpoint))
    {
      agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
        .GetChatClient(deploymentName)
        .AsAIAgent(instructions: "You are a helpful assistant.", tools: [AIFunctionFactory.Create(GetWeather)]);

      logger.LogInformation("AIAgent created with deployment '{deploymentName}' at endpoint '{endpoint}'", deploymentName, endpoint);
    }
    else
    {
      logger.LogError("AZURE_OPENAI_ENDPOINT environment variable is not set.");

      throw new InvalidOperationException("Set AZURE_OPENAI_ENDPOINT environment variable.");
    }
  }

  public async Task RunAsync()
  {
    _logger.LogInformation("Running agent with tool usage demo...");

    var prompt = "What is the weather like in Amsterdam?";
    Console.WriteLine("Question: {0}", prompt);

    Console.WriteLine(await agent.RunAsync(prompt));

    _logger.LogInformation("Finished running agent with tool usage demo.");
  }

}