using Microsoft.AspNetCore.Mvc;
using BackOffice.Domain.Interfaces;
using BackOffice.Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace BackOffice.Api.Controllers;

[ApiController]
[Route("api/chats")]
[Authorize]
public class ChatsController(
    IChatStore store,
    IChatService chatService,
    ILogger<ChatsController> logger) : ControllerBase
{
   public IChatStore Store { get; } = store;
   public IChatService ChatService { get; } = chatService;
   public ILogger<ChatsController> Logger { get; } = logger;

   [HttpGet]
    public async Task<ActionResult<List<ChatDto>>> GetAll()
    {
        return Ok(await Store.GetAllChatsAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChatDto>> Get(string id)
    {
        var chat = await Store.GetChatAsync(id);
        if (chat == null) return NotFound();
        return Ok(chat);
    }

    [HttpPost]
    public async Task<ActionResult<ChatDto>> Create([FromBody] CreateChatRequest request)
    {
        var chat = await Store.CreateChatAsync(request.Title);
        return CreatedAtAction(nameof(Get), new { id = chat.Id }, chat);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        if (!await Store.DeleteChatAsync(id)) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage(string id, [FromBody] SendMessageRequest request)
    {
        var chat = await Store.GetChatAsync(id);
        if (chat == null) return NotFound();

        // Save user message
        var userMessage = new MessageDto
        {
            Id = Guid.NewGuid().ToString(),
            ChatId = id,
            Role = "user",
            Content = request.Content,
            Timestamp = DateTime.UtcNow
        };
        await Store.AddMessageAsync(id, userMessage);

        // Process and save assistant response
        var assistantMessage = await ChatService.ProcessMessageAsync(id, request.Content);
        await Store.AddMessageAsync(id, assistantMessage);

        return Ok(assistantMessage);
    }
}
