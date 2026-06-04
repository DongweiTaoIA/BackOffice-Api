using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace BackOffice.Api.Hubs;

public partial class ChatHub(
    ILogger<ChatHub> logger,
    IChatStore chatStore,
    IChatService chatService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        LogClientConnected(Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
            LogClientDisconnectedWithError(exception, Context.ConnectionId);
        else
            LogClientDisconnected(Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(string chatId)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new HubException("Chat ID is required.");

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        LogClientJoinedChat(Context.ConnectionId, chatId);
    }

    public async Task LeaveChat(string chatId)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new HubException("Chat ID is required.");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        LogClientLeftChat(Context.ConnectionId, chatId);
    }

    public async Task SendMessage(string chatId, string content)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new HubException("Chat ID is required.");
        if (string.IsNullOrWhiteSpace(content))
            throw new HubException("Message content is required.");

        // Save user message
        var userMessage = new MessageDto
        {
            Id = Guid.NewGuid().ToString(),
            ChatId = chatId,
            Role = "user",
            Content = content,
            Timestamp = DateTime.UtcNow
        };
        await chatStore.AddMessageAsync(chatId, userMessage);
        await Clients.Group(chatId).SendAsync("ReceiveMessage", userMessage);

        // Process and send assistant response
        var assistantMessage = await chatService.ProcessMessageAsync(chatId, content);
        await chatStore.AddMessageAsync(chatId, assistantMessage);
        await Clients.Group(chatId).SendAsync("ReceiveMessage", assistantMessage);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Client connected: {ConnectionId}")]
    private partial void LogClientConnected(string connectionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Client disconnected: {ConnectionId}")]
    private partial void LogClientDisconnected(string connectionId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Client disconnected with error: {ConnectionId}")]
    private partial void LogClientDisconnectedWithError(Exception exception, string connectionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Client {ConnectionId} joined chat {ChatId}")]
    private partial void LogClientJoinedChat(string connectionId, string chatId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Client {ConnectionId} left chat {ChatId}")]
    private partial void LogClientLeftChat(string connectionId, string chatId);
}
