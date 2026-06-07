using MongoDB.Driver;
using BackOffice.Domain.Entities;

var client = new MongoClient("mongodb://admin:yourpassword@localhost:27017?authSource=admin");
var db = client.GetDatabase("backoffice");
var col = db.GetCollection<SupportTicket>("supportTickets");

var tickets = new List<SupportTicket>
{
    // Bugs
    new() { Id = "bug-001", Type = "bug", Title = "Login page crashes on Edge browser", Description = "When trying to log in using Edge 124, the page throws a JavaScript error and becomes unresponsive.", AuthorName = "Tao, Dongwei", AuthorEmail = "dongwei.tao@iafg.net", Status = "New", Severity = "High", StepsToReproduce = "1. Open Edge 124\n2. Navigate to login page\n3. Enter credentials\n4. Click Sign In\n5. Page freezes", Browser = "Edge 124", Os = "Windows 11", CreatedAt = DateTime.Parse("2026-06-04T09:15:00Z"), UpdatedAt = DateTime.Parse("2026-06-04T09:15:00Z") },
    new() { Id = "bug-002", Type = "bug", Title = "Contract PDF export missing footer", Description = "When exporting contracts to PDF, the footer with page numbers and company logo is completely missing.", AuthorName = "Martin, Sophie", AuthorEmail = "sophie.martin@iafg.net", Status = "InProgress", Severity = "Medium", StepsToReproduce = "1. Go to Contracts\n2. Select any contract\n3. Click Export PDF\n4. Footer is missing", Browser = "Chrome 125", Os = "Windows 11", CreatedAt = DateTime.Parse("2026-06-03T14:30:00Z"), UpdatedAt = DateTime.Parse("2026-06-05T10:00:00Z") },
    new() { Id = "bug-003", Type = "bug", Title = "Dashboard charts not loading for Claims module", Description = "The pie chart and bar chart on the Claims dashboard show a spinner indefinitely. Network tab shows 504 gateway timeout.", AuthorName = "Chen, Wei", AuthorEmail = "wei.chen@iafg.net", Status = "Resolved", Severity = "Critical", Browser = "Chrome 125", Os = "macOS 14", CreatedAt = DateTime.Parse("2026-06-02T08:45:00Z"), UpdatedAt = DateTime.Parse("2026-06-05T16:00:00Z") },
    // Features
    new() { Id = "feat-001", Type = "feature", Title = "Dark mode support for the entire application", Description = "Many users work late and would benefit from a dark mode toggle in user preferences.", AuthorName = "Tao, Dongwei", AuthorEmail = "dongwei.tao@iafg.net", Status = "New", Priority = "Nice-to-have", UseCase = "Users in low-light environments experience eye strain. Dark mode would improve comfort during evening shifts.", CreatedAt = DateTime.Parse("2026-06-03T10:00:00Z"), UpdatedAt = DateTime.Parse("2026-06-04T11:00:00Z") },
    new() { Id = "feat-002", Type = "feature", Title = "Bulk export contracts to Excel", Description = "Need ability to select multiple contracts and export them in a single Excel file.", AuthorName = "Lavoie, Marc", AuthorEmail = "marc.lavoie@iafg.net", Status = "New", Priority = "Must-have", UseCase = "During quarterly audits, we need to export 50+ contracts at once. Currently one by one takes hours.", CreatedAt = DateTime.Parse("2026-06-05T13:20:00Z"), UpdatedAt = DateTime.Parse("2026-06-05T13:20:00Z") },
    new() { Id = "feat-003", Type = "feature", Title = "Email notifications for ticket status changes", Description = "Send email notifications to ticket author and commenters when status changes.", AuthorName = "Chen, Wei", AuthorEmail = "wei.chen@iafg.net", Status = "InProgress", Priority = "Nice-to-have", UseCase = "Users have to manually check the portal. Notifications would save time.", CreatedAt = DateTime.Parse("2026-06-01T09:00:00Z"), UpdatedAt = DateTime.Parse("2026-06-04T14:00:00Z") },
    // Help
    new() { Id = "help-001", Type = "help", Title = "How to configure SSO for new team members?", Description = "We have 3 new team members joining next week. What are the steps to set up their SSO access?", AuthorName = "Tremblay, Julie", AuthorEmail = "julie.tremblay@iafg.net", Status = "New", Urgency = "Non-blocking", RelatedModule = "Authentication", CreatedAt = DateTime.Parse("2026-06-05T15:30:00Z"), UpdatedAt = DateTime.Parse("2026-06-05T15:30:00Z") },
    new() { Id = "help-002", Type = "help", Title = "Cannot access Claims module - permission error", Description = "Getting 403 Forbidden when accessing Claims. Had access last week. Role should include Claims read/write.", AuthorName = "Martin, Sophie", AuthorEmail = "sophie.martin@iafg.net", Status = "InProgress", Urgency = "Blocking", RelatedModule = "Claims", CreatedAt = DateTime.Parse("2026-06-06T08:00:00Z"), UpdatedAt = DateTime.Parse("2026-06-06T08:30:00Z") },
    new() { Id = "help-003", Type = "help", Title = "Need guidance on One Statement report generation", Description = "Need to generate quarterly One Statement report but unsure which parameters to use. Docs seem outdated.", AuthorName = "Lavoie, Marc", AuthorEmail = "marc.lavoie@iafg.net", Status = "Resolved", Urgency = "Non-blocking", RelatedModule = "One Statement", CreatedAt = DateTime.Parse("2026-06-04T09:00:00Z"), UpdatedAt = DateTime.Parse("2026-06-04T11:30:00Z") }
};

// Delete existing test data first
var ids = tickets.Select(t => t.Id).ToList();
await col.DeleteManyAsync(Builders<SupportTicket>.Filter.In(t => t.Id, ids));
await col.InsertManyAsync(tickets);
Console.WriteLine($"Inserted {tickets.Count} test tickets successfully!");
