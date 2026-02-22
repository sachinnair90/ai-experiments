namespace Workflows;

public static class AgentConfig
{
    public static string? AzureEndpoint => Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
    public static string? AzureDeploymentName => Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
        ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");
    public static string LogLevel => Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information";
}
