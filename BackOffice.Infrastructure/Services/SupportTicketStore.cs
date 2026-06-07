using BackOffice.Domain.Entities;
using BackOffice.Domain.Interfaces;
using MongoDB.Driver;

namespace BackOffice.Infrastructure.Services;

public class SupportTicketStore : ISupportTicketStore
{
    private readonly IMongoCollection<SupportTicket> _collection;

    public SupportTicketStore(IMongoDatabase database)
    {
        _collection = database.GetCollection<SupportTicket>("supportTickets");
    }

    public async Task<List<SupportTicket>> GetByTypeAsync(string type, string? search = null)
    {
        var filterBuilder = Builders<SupportTicket>.Filter;
        var filter = filterBuilder.Eq(t => t.Type, type);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(t => t.Title, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                filterBuilder.Regex(t => t.Description, new MongoDB.Bson.BsonRegularExpression(search, "i"))
            );
            filter = filterBuilder.And(filter, searchFilter);
        }

        return await _collection
            .Find(filter)
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SupportTicket>> GetByAuthorAsync(string email, string type)
    {
        var filter = Builders<SupportTicket>.Filter.And(
            Builders<SupportTicket>.Filter.Eq(t => t.AuthorEmail, email),
            Builders<SupportTicket>.Filter.Eq(t => t.Type, type)
        );

        return await _collection
            .Find(filter)
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupportTicket?> GetByIdAsync(string id)
    {
        return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task<SupportTicket> CreateAsync(SupportTicket ticket)
    {
        ticket.Id = Guid.NewGuid().ToString();
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(ticket);
        return ticket;
    }

    public async Task<SupportTicket?> UpdateAsync(SupportTicket ticket)
    {
        ticket.UpdatedAt = DateTime.UtcNow;
        var filter = Builders<SupportTicket>.Filter.Eq(t => t.Id, ticket.Id);
        var result = await _collection.ReplaceOneAsync(filter, ticket);
        if (result.ModifiedCount == 0) return null;
        return ticket;
    }

    public async Task<SupportTicket?> UpdateStatusAsync(string id, string status)
    {
        var update = Builders<SupportTicket>.Update
            .Set(t => t.Status, status)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<SupportTicket> { ReturnDocument = ReturnDocument.After };
        return await _collection.FindOneAndUpdateAsync(t => t.Id == id, update, options);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(t => t.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<SupportTicket?> AddCommentAsync(string id, SupportTicketComment comment)
    {
        var update = Builders<SupportTicket>.Update
            .Push(t => t.Comments, comment)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<SupportTicket> { ReturnDocument = ReturnDocument.After };
        return await _collection.FindOneAndUpdateAsync(t => t.Id == id, update, options);
    }

    public async Task<bool> DeleteCommentAsync(string ticketId, string commentId)
    {
        var update = Builders<SupportTicket>.Update
            .PullFilter(t => t.Comments, c => c.Id == commentId)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(t => t.Id == ticketId, update);
        return result.ModifiedCount > 0;
    }
}
