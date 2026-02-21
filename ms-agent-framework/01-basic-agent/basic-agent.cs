using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using ms_agent_framework;

namespace BasicAgent;

public class BasicAgent : ISubApplication
{
  private readonly AIAgent agent;
  private readonly ILogger<BasicAgent> logger;

  public BasicAgent(ILogger<BasicAgent> logger)
  {
    this.logger = logger;

    var endpoint = AgentConfig.AzureEndpoint;
    var deploymentName = AgentConfig.AzureDeploymentName ?? "gpt-4o-mini";
    var name = "BasicAgent";
    // expose identity for Program discovery
    this.AppName = nameof(BasicAgent);
    this.AppDescription = "Basic demo agent using Azure OpenAI via MS Agent Framework SDK.";
    if (!string.IsNullOrEmpty(endpoint))
    {
      agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
        .GetChatClient(deploymentName)
        .AsAIAgent(instructions: "You are a friendly assistant. Keep your answers brief.", name: name);

      logger.LogInformation("AIAgent created with deployment '{deploymentName}' at endpoint '{endpoint}'", deploymentName, endpoint);
    }
    else
    {
      logger.LogError("AZURE_OPENAI_ENDPOINT environment variable is not set.");

      throw new InvalidOperationException("Set AZURE_OPENAI_ENDPOINT environment variable.");
    }
  }

  public string AppName { get; private set; }
  public string AppDescription { get; private set; }

  public async Task RunAsync()
  {
    await NonStreamingAgentRunAsync();
    await StreamAsync();
  }

  public async Task NonStreamingAgentRunAsync()
  {
    logger.LogInformation("Running synchronous agent with a sample prompt...");
    var prompt = "What is the largest city in France?";
    Console.WriteLine("Question: {0}", prompt);
    var answer = await agent.RunAsync(prompt);
    Console.WriteLine("Answer: {0}", answer);

    logger.LogInformation("Finished running synchronous agent.");
  }

  public async Task StreamAsync()
  {
    logger.LogInformation("Running streaming agent with a sample prompt...");
    var prompt = "Tell me a one-sentence fun fact.";
    Console.WriteLine("Starting streaming for prompt: {0}", prompt);
    await foreach (var update in agent.RunStreamingAsync(prompt))
    {
      Console.Write(update);
      await Task.Delay(50); // slow down the output so we can see it stream in
    }

    Console.WriteLine(); // add a newline after streaming is done

    logger.LogInformation("Finished streaming prompt: {prompt}", prompt);
  }
}
