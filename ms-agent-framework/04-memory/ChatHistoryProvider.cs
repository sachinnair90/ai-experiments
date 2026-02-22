using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Memory;

public class CustomChatHistoryProvider : ChatHistoryProvider
{
  private readonly ProviderSessionState<State> _sessionState;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomChatHistoryProvider"/> class.
    /// </summary>
    /// <param name="options">
    /// Optional configuration options that control the provider's behavior, including state initialization,
    /// message reduction, and serialization settings. If <see langword="null"/>, default settings will be used.
    /// </param>
    public CustomChatHistoryProvider()
        : base()
    {
        Func<AgentSession?, State> stateInitializer = _ => new State();
        this._sessionState = new ProviderSessionState<State>(
            stateInitializer,
            this.GetType().Name,
            null);
        this.ChatReducer = null;
        this.ReducerTriggerEvent = InMemoryChatHistoryProviderOptions.ChatReducerTriggerEvent.BeforeMessagesRetrieval;
    }

    /// <inheritdoc />
    public override string StateKey => this._sessionState.StateKey;

    /// <summary>
    /// Gets the chat reducer used to process or reduce chat messages. If null, no reduction logic will be applied.
    /// </summary>
    public IChatReducer? ChatReducer { get; }

    /// <summary>
    /// Gets the event that triggers the reducer invocation in this provider.
    /// </summary>
    public InMemoryChatHistoryProviderOptions.ChatReducerTriggerEvent ReducerTriggerEvent { get; }

    /// <summary>
    /// Gets the chat messages stored for the specified session.
    /// </summary>
    /// <param name="session">The agent session containing the state.</param>
    /// <returns>A list of chat messages, or an empty list if no state is found.</returns>
    public List<ChatMessage> GetMessages(AgentSession? session)
        => this._sessionState.GetOrInitializeState(session).Messages;

    /// <summary>
    /// Sets the chat messages for the specified session.
    /// </summary>
    /// <param name="session">The agent session containing the state.</param>
    /// <param name="messages">The messages to store.</param>
    /// <exception cref="ArgumentNullException"><paramref name="messages"/> is <see langword="null"/>.</exception>
    public void SetMessages(AgentSession? session, List<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages, nameof(messages));

        var state = this._sessionState.GetOrInitializeState(session);
        state.Messages = messages;
    }

    /// <inheritdoc />
    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        var state = this._sessionState.GetOrInitializeState(context.Session);

        if (this.ReducerTriggerEvent is InMemoryChatHistoryProviderOptions.ChatReducerTriggerEvent.BeforeMessagesRetrieval && this.ChatReducer is not null)
        {
            state.Messages = (await this.ChatReducer.ReduceAsync(state.Messages, cancellationToken).ConfigureAwait(false)).ToList();
        }

        return state.Messages;
    }

    /// <inheritdoc />
    protected override async ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        var state = this._sessionState.GetOrInitializeState(context.Session);

        // Add request and response messages to the provider
        var allNewMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []);
        state.Messages.AddRange(allNewMessages);

        if (this.ReducerTriggerEvent is InMemoryChatHistoryProviderOptions.ChatReducerTriggerEvent.AfterMessageAdded && this.ChatReducer is not null)
        {
            state.Messages = (await this.ChatReducer.ReduceAsync(state.Messages, cancellationToken).ConfigureAwait(false)).ToList();
        }
    }

    /// <summary>
    /// Represents the state of a <see cref="CustomChatHistoryProvider"/> stored in the <see cref="AgentSession.StateBag"/>.
    /// </summary>
    public sealed class State
    {
        /// <summary>
        /// Gets or sets the list of chat messages.
        /// </summary>
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = [];
    }
}