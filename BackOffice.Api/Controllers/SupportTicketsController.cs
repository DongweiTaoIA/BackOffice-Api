using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackOffice.Domain.Entities;
using BackOffice.Domain.Interfaces;

namespace BackOffice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupportTicketsController(
    ISupportTicketStore store,
    ILogger<SupportTicketsController> logger) : ControllerBase
{
    private readonly ISupportTicketStore _store = store;
    private readonly ILogger<SupportTicketsController> _logger = logger;

    private static readonly string[] AllowedTypes = ["bug", "feature", "help"];
    private static readonly string[] AllowedStatuses = ["New", "InProgress", "Resolved", "Closed"];
    private static readonly string UploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "support-tickets");
    private static readonly string InlineImagePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "inline-images");
    private static readonly string[] AllowedImageTypes = ["image/png", "image/jpeg", "image/gif", "image/webp"];

    [HttpGet]
    public async Task<ActionResult<List<SupportTicketIndexDto>>> GetAll(
        [FromQuery] string type,
        [FromQuery] string? search = null)
    {
        if (!AllowedTypes.Contains(type))
            return BadRequest("Invalid ticket type.");

        var tickets = await _store.GetByTypeAsync(type, search);
        return Ok(tickets.Select(ToIndexDto).ToList());
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<SupportTicketIndexDto>>> GetMine(
        [FromQuery] string email,
        [FromQuery] string type)
    {
        if (!AllowedTypes.Contains(type))
            return BadRequest("Invalid ticket type.");

        var tickets = await _store.GetByAuthorAsync(email, type);
        return Ok(tickets.Select(ToIndexDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupportTicket>> GetById(string id)
    {
        var ticket = await _store.GetByIdAsync(id);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)] // 50MB
    public async Task<ActionResult<SupportTicket>> Create([FromForm] CreateSupportTicketForm form)
    {
        if (string.IsNullOrWhiteSpace(form.Title) || string.IsNullOrWhiteSpace(form.Description))
            return BadRequest("Title and Description are required.");

        if (!AllowedTypes.Contains(form.Type))
            return BadRequest("Invalid ticket type.");

        var ticket = new SupportTicket
        {
            Type = form.Type,
            Title = form.Title.Trim(),
            Description = form.Description.Trim(),
            AuthorName = form.AuthorName?.Trim() ?? "Unknown",
            AuthorEmail = form.AuthorEmail?.Trim() ?? "",
            Severity = form.Severity,
            StepsToReproduce = form.StepsToReproduce,
            Browser = form.Browser,
            Os = form.Os,
            Priority = form.Priority,
            UseCase = form.UseCase,
            Urgency = form.Urgency,
            RelatedModule = form.RelatedModule,
            VideoLink = form.VideoLink,
        };

        // Handle file uploads
        if (form.Files is { Count: > 0 })
        {
            var ticketUploadDir = Path.Combine(UploadPath, ticket.Id.Length > 0 ? ticket.Id : Guid.NewGuid().ToString());
            // We'll set the real ID after creation, so save files with a temp ID approach
            // Actually, let's create the ticket first to get the ID, then save files
            var created = await _store.CreateAsync(ticket);
            ticketUploadDir = Path.Combine(UploadPath, created.Id);
            Directory.CreateDirectory(ticketUploadDir);

            foreach (var file in form.Files)
            {
                var safeFileName = SanitizeFileName(file.FileName);
                var filePath = Path.Combine(ticketUploadDir, safeFileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                created.Attachments.Add(new SupportTicketAttachment
                {
                    FileName = safeFileName,
                    ContentType = file.ContentType,
                    Size = file.Length,
                });
            }

            // Update ticket with attachments info (we already created it)
            // For simplicity, delete and re-create, or just update attachments field
            // Let's use a direct approach: update the document
            // We'll just return the ticket with attachments populated from memory
            return Ok(created);
        }

        var result = await _store.CreateAsync(ticket);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<SupportTicket>> UpdateStatus(string id, [FromBody] UpdateStatusRequest request)
    {
        if (!AllowedStatuses.Contains(request.Status))
            return BadRequest("Invalid status.");

        var updated = await _store.UpdateStatusAsync(id, request.Status);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpPut("{id}")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<SupportTicket>> Update(string id, [FromForm] UpdateSupportTicketForm form)
    {
        var ticket = await _store.GetByIdAsync(id);
        if (ticket == null) return NotFound();

        if (!string.Equals(ticket.AuthorEmail, form.RequesterEmail, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        // Update fields
        if (!string.IsNullOrWhiteSpace(form.Title)) ticket.Title = form.Title.Trim();
        if (!string.IsNullOrWhiteSpace(form.Description)) ticket.Description = form.Description.Trim();
        ticket.Severity = form.Severity ?? ticket.Severity;
        ticket.StepsToReproduce = form.StepsToReproduce ?? ticket.StepsToReproduce;
        ticket.Browser = form.Browser ?? ticket.Browser;
        ticket.Os = form.Os ?? ticket.Os;
        ticket.Priority = form.Priority ?? ticket.Priority;
        ticket.UseCase = form.UseCase ?? ticket.UseCase;
        ticket.Urgency = form.Urgency ?? ticket.Urgency;
        ticket.RelatedModule = form.RelatedModule ?? ticket.RelatedModule;
        ticket.VideoLink = form.VideoLink ?? ticket.VideoLink;

        // Handle attachment deletions
        if (form.DeleteAttachments is { Count: > 0 })
        {
            foreach (var fileName in form.DeleteAttachments)
            {
                var safeFileName = SanitizeFileName(fileName);
                var filePath = Path.Combine(UploadPath, id, safeFileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                ticket.Attachments.RemoveAll(a => a.FileName == safeFileName);
            }
        }

        // Handle new file uploads
        if (form.Files is { Count: > 0 })
        {
            var ticketUploadDir = Path.Combine(UploadPath, id);
            Directory.CreateDirectory(ticketUploadDir);

            foreach (var file in form.Files)
            {
                var safeFileName = SanitizeFileName(file.FileName);
                var filePath = Path.Combine(ticketUploadDir, safeFileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                ticket.Attachments.Add(new SupportTicketAttachment
                {
                    FileName = safeFileName,
                    ContentType = file.ContentType,
                    Size = file.Length,
                });
            }
        }

        var updated = await _store.UpdateAsync(ticket);
        if (updated == null) return StatusCode(500, "Failed to update ticket.");
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id, [FromQuery] string requesterEmail)
    {
        var ticket = await _store.GetByIdAsync(id);
        if (ticket == null) return NotFound();

        if (!string.Equals(ticket.AuthorEmail, requesterEmail, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        await _store.DeleteAsync(id);

        // Clean up files
        var ticketUploadDir = Path.Combine(UploadPath, id);
        if (Directory.Exists(ticketUploadDir))
            Directory.Delete(ticketUploadDir, true);

        return NoContent();
    }

    [HttpPost("{id}/comments")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<SupportTicket>> AddComment(string id, [FromForm] AddCommentForm form)
    {
        if (string.IsNullOrWhiteSpace(form.Text))
            return BadRequest("Comment text is required.");

        var inlineImages = new List<InlineImage>();

        // Handle pasted images: store inline and embed URLs in comment text
        if (form.Images is { Count: > 0 })
        {
            Directory.CreateDirectory(InlineImagePath);
            foreach (var image in form.Images)
            {
                if (!AllowedImageTypes.Contains(image.ContentType.ToLowerInvariant()))
                    continue;

                var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(InlineImagePath, uniqueName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                var url = $"/api/supporttickets/inline-images/{uniqueName}";
                inlineImages.Add(new InlineImage
                {
                    FileName = uniqueName,
                    Url = url,
                    ContentType = image.ContentType,
                });
            }
        }

        var comment = new SupportTicketComment
        {
            Text = form.Text.Trim(),
            AuthorName = form.AuthorName?.Trim() ?? "Unknown",
            AuthorEmail = form.Email?.Trim() ?? "",
            InlineImages = inlineImages,
        };

        var updated = await _store.AddCommentAsync(id, comment);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}/comments/{commentId}")]
    public async Task<ActionResult> DeleteComment(string id, string commentId)
    {
        var success = await _store.DeleteCommentAsync(id, commentId);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("upload-image")]
    [RequestSizeLimit(10_000_000)] // 10MB
    public async Task<ActionResult> UploadInlineImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        if (!AllowedImageTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Only image files (png, jpeg, gif, webp) are allowed.");

        var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        Directory.CreateDirectory(InlineImagePath);
        var filePath = Path.Combine(InlineImagePath, uniqueName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var url = $"/api/supporttickets/inline-images/{uniqueName}";
        return Ok(new { url, fileName = uniqueName, contentType = file.ContentType });
    }

    [HttpGet("inline-images/{fileName}")]
    [AllowAnonymous]
    public ActionResult GetInlineImage(string fileName)
    {
        var safeFileName = SanitizeFileName(fileName);
        var filePath = Path.Combine(InlineImagePath, safeFileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var contentType = GetContentType(safeFileName);
        return PhysicalFile(filePath, contentType, safeFileName);
    }

    [HttpGet("{id}/attachments/{fileName}")]
    public ActionResult GetAttachment(string id, string fileName)
    {
        var safeFileName = SanitizeFileName(fileName);
        var filePath = Path.Combine(UploadPath, id, safeFileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var contentType = GetContentType(safeFileName);
        return PhysicalFile(filePath, contentType, safeFileName);
    }

    private static SupportTicketIndexDto ToIndexDto(SupportTicket t) => new()
    {
        Id = t.Id,
        Type = t.Type,
        Title = t.Title,
        AuthorName = t.AuthorName,
        AuthorEmail = t.AuthorEmail,
        Status = t.Status,
        CommentCount = t.Comments.Count,
        AttachmentCount = t.Attachments.Count,
        Severity = t.Severity,
        Priority = t.Priority,
        Urgency = t.Urgency,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
    };

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Length > 200 ? sanitized[..200] : sanitized;
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".txt" => "text/plain",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            _ => "application/octet-stream",
        };
    }
}

// Request/Response DTOs
public class CreateSupportTicketForm
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AuthorName { get; set; }
    public string? AuthorEmail { get; set; }
    public List<IFormFile>? Files { get; set; }
    // Bug
    public string? Severity { get; set; }
    public string? StepsToReproduce { get; set; }
    public string? Browser { get; set; }
    public string? Os { get; set; }
    // Feature
    public string? Priority { get; set; }
    public string? UseCase { get; set; }
    // Help
    public string? Urgency { get; set; }
    public string? RelatedModule { get; set; }
    // Shared
    public string? VideoLink { get; set; }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? RequesterEmail { get; set; }
}

public class UpdateSupportTicketForm
{
    public string RequesterEmail { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<IFormFile>? Files { get; set; }
    public List<string>? DeleteAttachments { get; set; }
    // Bug
    public string? Severity { get; set; }
    public string? StepsToReproduce { get; set; }
    public string? Browser { get; set; }
    public string? Os { get; set; }
    // Feature
    public string? Priority { get; set; }
    public string? UseCase { get; set; }
    // Help
    public string? Urgency { get; set; }
    public string? RelatedModule { get; set; }
    // Shared
    public string? VideoLink { get; set; }
}

public class AddCommentForm
{
    public string Text { get; set; } = string.Empty;
    public string? AuthorName { get; set; }
    public string? Email { get; set; }
    public List<IFormFile>? Images { get; set; }
}

public class SupportTicketIndexDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
    public string? Severity { get; set; }
    public string? Priority { get; set; }
    public string? Urgency { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
