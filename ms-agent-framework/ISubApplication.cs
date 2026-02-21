using System.Threading.Tasks;

namespace ms_agent_framework;

public interface ISubApplication
{
    string AppName { get; }
    string AppDescription { get; }
    Task RunAsync();
}
