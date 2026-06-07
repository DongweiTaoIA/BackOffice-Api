using BackOffice.Domain.Entities;

namespace BackOffice.Domain.Interfaces;

public interface ISupportTicketStore
{
    Task<List<SupportTicket>> GetByTypeAsync(string type, string? search = null);
    Task<List<SupportTicket>> GetByAuthorAsync(string email, string type);
    Task<SupportTicket?> GetByIdAsync(string id);
    Task<SupportTicket> CreateAsync(SupportTicket ticket);
    Task<SupportTicket?> UpdateAsync(SupportTicket ticket);
    Task<SupportTicket?> UpdateStatusAsync(string id, string status);
    Task<bool> DeleteAsync(string id);
    Task<SupportTicket?> AddCommentAsync(string id, SupportTicketComment comment);
    Task<bool> DeleteCommentAsync(string ticketId, string commentId);
}
