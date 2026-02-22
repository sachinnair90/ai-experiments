using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ms_agent_framework;
using OpenAI.Chat;

namespace MultiTurnConversation;

public class MultiTurnConversation : ISubApplication
{
  private readonly ILogger<MultiTurnConversation> _logger;
  private readonly AIAgent agent;

  public string AppName { get; private set; }

  public string AppDescription { get; private set; }

  public MultiTurnConversation(ILogger<MultiTurnConversation> logger)
  {
    _logger = logger;

    var endpoint = AgentConfig.AzureEndpoint;
    var deploymentName = AgentConfig.AzureDeploymentName ?? "gpt-4o-mini";
    // expose identity for Program discovery
    this.AppName = nameof(MultiTurnConversation);
    this.AppDescription = "Demo agent showing how to have a multi-turn conversation with Azure OpenAI via MS Agent Framework SDK.";
    if (!string.IsNullOrEmpty(endpoint))
    {
      agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
        .GetChatClient(deploymentName)
        .AsAIAgent(instructions: "You are a helpful assistant. Keep your answers brief.", name: "ConversationAgent");

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
    _logger.LogInformation("Running agent with multi-turn conversation demo...");

    // Create a session to maintain conversation history
    AgentSession session = await agent.CreateSessionAsync();

    var prompt1 = "My name is Sachin and I love hiking.";
    Console.WriteLine($"User: {prompt1}");
    // First turn
    Console.WriteLine(await agent.RunAsync(prompt1, session));

    Console.WriteLine(); // Blank line for readability

    // Second turn — the agent remembers the user's name and hobby
    var prompt2 = "What do you remember about me?";
    Console.WriteLine($"User: {prompt2}");
    Console.WriteLine(await agent.RunAsync(prompt2, session));

    Console.WriteLine(); // Blank line for readability

    _logger.LogInformation("Finished running agent with multi-turn conversation demo.");
  }

}