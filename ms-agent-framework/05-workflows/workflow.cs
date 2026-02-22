using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using ms_agent_framework;
using Microsoft.Agents.AI.Workflows;

namespace Workflows;

public class Workflow : ISubApplication
{
  private readonly ILogger<Workflow> _logger;
  private readonly Microsoft.Agents.AI.Workflows.Workflow workflow;

  public string AppName { get; private set; }

  public string AppDescription { get; private set; }


  // Step 1: Convert text to uppercase
  internal sealed class UpperCase() : Executor<string, string>("UpperCaseExecutor")
  {
    public async override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
      return await new ValueTask<string>(message.ToUpper());
    }
  }

  /// <summary>
  /// Second executor: reverses the input text and completes the workflow.
  /// </summary>
  internal sealed class ReverseTextExecutor() : Executor<string, string>("ReverseTextExecutor")
  {
    /// <summary>
    /// Processes the input message by reversing the text.
    /// </summary>
    /// <param name="message">The input text to reverse</param>
    /// <param name="context">Workflow context for accessing workflow services and adding events</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The input text reversed</returns>
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
      // Because we do not suppress it, the returned result will be yielded as an output from this executor.
      return ValueTask.FromResult(string.Concat(message.Reverse()));
    }
  }

  public Workflow(ILogger<Workflow> logger)
  {
    _logger = logger;

    // expose identity for Program discovery
    this.AppName = nameof(Workflow);
    this.AppDescription = "Demo agent showing how to use memory with Azure OpenAI via MS Agent Framework SDK.";

    // Create the executors that will be used in the workflow
    var uppercase = new UpperCase();
    var reverse = new ReverseTextExecutor();

    // Build the workflow by connecting executors sequentially
    WorkflowBuilder builder = new(uppercase);
    builder.AddEdge(uppercase, reverse).WithOutputFrom(reverse);
    workflow = builder.Build();
  }

  public async Task RunAsync()
  {
    _logger.LogInformation("Running agent with memory demo...");

    // Execute the workflow with input data
    await using Run run = await InProcessExecution.RunAsync(workflow, "Hello, World!");
    foreach (WorkflowEvent evt in run.NewEvents)
    {
        if (evt is ExecutorCompletedEvent executorComplete)
        {
            Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
        }
    }

    Console.WriteLine(); // Blank line for readability

    _logger.LogInformation("Finished running agent with memory demo.");
  }

}