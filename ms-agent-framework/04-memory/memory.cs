using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ms_agent_framework;
using OpenAI.Chat;

namespace Memory;

public class Memory : ISubApplication
{
    private readonly ILogger<Memory> _logger;
    private readonly AIAgent? agent;

    public string AppName { get; private set; }

    public string AppDescription { get; private set; }

    public Memory(ILogger<Memory> logger)
    {
        _logger = logger;

        var endpoint = AgentConfig.AzureEndpoint;
        var deploymentName = AgentConfig.AzureDeploymentName ?? "gpt-4o-mini";
        // expose identity for Program discovery
        this.AppName = nameof(Memory);
        this.AppDescription = "Demo agent showing how to use memory with Azure OpenAI via MS Agent Framework SDK.";
        if (!string.IsNullOrEmpty(endpoint))
        {
            agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
                .GetChatClient(deploymentName)
                .AsAIAgent(new ChatClientAgentOptions()
                {
                    ChatOptions = new() { Instructions = "You are a helpful assistant." },
                    ChatHistoryProvider = new CustomChatHistoryProvider(),
                });

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
        _logger.LogInformation("Running agent with memory demo...");

        if (agent == null)
            throw new InvalidOperationException("Agent was not initialized.");

        // Create a session to maintain conversation history
        AgentSession session = await agent.CreateSessionAsync();

        var prompt1 = "Hello! What's the square root of 9?";
        Console.WriteLine($"User: {prompt1}");
        Console.WriteLine(await agent.RunAsync(prompt1, session));
        Console.WriteLine(); // Blank line for readability

        var prompt2 = "My name is Alice";
        Console.WriteLine($"User: {prompt2}");
        Console.WriteLine(await agent.RunAsync(prompt2, session));
        Console.WriteLine(); // Blank line for readability

        var prompt3 = "What is my name?";
        Console.WriteLine($"User: {prompt3}");
        Console.WriteLine(await agent.RunAsync(prompt3, session));

        Console.WriteLine(); // Blank line for readability

        _logger.LogInformation("Finished running agent with memory demo.");
    }

}