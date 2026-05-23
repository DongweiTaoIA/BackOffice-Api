using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackOffice.Api.Models;
using BackOffice.Api.Services;

namespace BackOffice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationStore _store;
    private readonly IChatService _chatService;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(
        IConversationStore store,
        IChatService chatService,
        ILogger<ConversationsController> logger)
    {
        _store = store;
        _chatService = chatService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<List<ConversationDto>> GetAll()
    {
        return Ok(_store.GetAllConversations());
    }

    [HttpGet("{id}")]
    public ActionResult<ConversationDto> Get(string id)
    {
        var conversation = _store.GetConversation(id);
        if (conversation == null) return NotFound();
        return Ok(conversation);
    }

    [HttpPost]
    public ActionResult<ConversationDto> Create([FromBody] CreateConversationRequest request)
    {
        var conversation = _store.CreateConversation(request.Title);
        return CreatedAtAction(nameof(Get), new { id = conversation.Id }, conversation);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(string id)
    {
        if (!_store.DeleteConversation(id)) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage(string id, [FromBody] SendMessageRequest request)
    {
        var conversation = _store.GetConversation(id);
        if (conversation == null) return NotFound();

        // Save user message
        var userMessage = new MessageDto
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = id,
            Role = "user",
            Content = request.Content,
            Timestamp = DateTime.UtcNow
        };
        _store.AddMessage(id, userMessage);

        // Process and save assistant response
        var assistantMessage = await _chatService.ProcessMessageAsync(id, request.Content);
        _store.AddMessage(id, assistantMessage);

        return Ok(assistantMessage);
    }
}
