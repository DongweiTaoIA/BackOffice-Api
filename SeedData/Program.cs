using MongoDB.Driver;
using BackOffice.Domain.Entities;
using System.Drawing;
using System.Drawing.Imaging;

var client = new MongoClient("mongodb://admin:yourpassword@localhost:27017?authSource=admin");
var db = client.GetDatabase("backoffice");
var col = db.GetCollection<SupportTicket>("supportTickets");

// Create placeholder images for attachments
var uploadsBase = Path.Combine(Directory.GetCurrentDirectory(), "..", "BackOffice.Api", "uploads", "support-tickets");

void CreatePlaceholderImage(string ticketId, string fileName, string labelText)
{
    var dir = Path.Combine(uploadsBase, ticketId);
    Directory.CreateDirectory(dir);
    var filePath = Path.Combine(dir, fileName);
    if (File.Exists(filePath)) return;

    using var bmp = new Bitmap(640, 400);
    using var g = Graphics.FromImage(bmp);
    g.Clear(Color.FromArgb(245, 245, 250));
    g.DrawRectangle(new Pen(Color.FromArgb(200, 200, 210), 2), 2, 2, 636, 396);
    using var font = new Font("Segoe UI", 13, FontStyle.Bold);
    using var smallFont = new Font("Segoe UI", 10);
    g.DrawString(labelText, font, Brushes.DarkSlateGray, new RectangleF(20, 20, 600, 50));
    g.DrawString($"Screenshot: {fileName}", smallFont, Brushes.Gray, new RectangleF(20, 365, 600, 30));
    // Fake app header
    g.FillRectangle(new SolidBrush(Color.FromArgb(99, 102, 241)), 20, 70, 600, 36);
    g.DrawString("BackOffice Application", new Font("Segoe UI", 10, FontStyle.Bold), Brushes.White, 30, 78);
    // Content area
    g.FillRectangle(Brushes.White, 20, 110, 600, 245);
    g.DrawRectangle(Pens.LightGray, 20, 110, 600, 245);
    // Error banner
    g.FillRectangle(new SolidBrush(Color.FromArgb(254, 226, 226)), 35, 125, 570, 35);
    g.DrawString("⚠ Issue visible in this area", smallFont, new SolidBrush(Color.FromArgb(185, 28, 28)), 50, 133);
    // Some fake content lines
    var lineY = 180;
    var rng = new Random(ticketId.GetHashCode());
    for (int i = 0; i < 6; i++)
    {
        var width = rng.Next(200, 550);
        g.FillRectangle(new SolidBrush(Color.FromArgb(229, 231, 235)), 35, lineY, width, 12);
        lineY += 22;
    }
    bmp.Save(filePath, ImageFormat.Png);
}

var tickets = new List<SupportTicket>
{
    // === BUG TICKETS (12 new) ===
    new()
    {
        Id = "bug-004", Type = "bug",
        Title = "Search results show duplicate entries",
        Description = "When searching for contracts by name, some results appear 2-3 times in the list. This happens consistently with names containing special characters like accents.",
        AuthorName = "Bergeron, Luc", AuthorEmail = "luc.bergeron@iafg.net",
        Status = "New", Severity = "Medium",
        StepsToReproduce = "1. Go to Contracts search\n2. Type 'Côté' in search box\n3. Notice duplicate results",
        Browser = "Chrome 125", Os = "Windows 11",
        Attachments = [new() { FileName = "duplicate-search.png", ContentType = "image/png", Size = 45000 }],
        CreatedAt = DateTime.Parse("2026-06-05T11:20:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T11:20:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-005", Type = "bug",
        Title = "Date picker shows wrong month on first open",
        Description = "The date picker component initially shows January 2020 instead of the current month. User has to manually navigate to the correct date.",
        AuthorName = "Tremblay, Julie", AuthorEmail = "julie.tremblay@iafg.net",
        Status = "New", Severity = "Low",
        Browser = "Firefox 126", Os = "Windows 10",
        Attachments = [new() { FileName = "datepicker-wrong-month.png", ContentType = "image/png", Size = 32000 }],
        CreatedAt = DateTime.Parse("2026-06-05T14:45:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T14:45:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-006", Type = "bug",
        Title = "File upload fails silently for files > 10MB",
        Description = "When uploading attachments larger than 10MB, the upload appears to complete but the file is never stored. No error message shown to user.",
        AuthorName = "Lavoie, Marc", AuthorEmail = "marc.lavoie@iafg.net",
        Status = "InProgress", Severity = "High",
        StepsToReproduce = "1. Create a new ticket\n2. Attach a file > 10MB\n3. Submit the ticket\n4. Open ticket - attachment is missing",
        Browser = "Chrome 125", Os = "Windows 11",
        Comments = [new() { Id = "c10", Text = "Confirmed. The nginx proxy has a 10MB limit. Fixing config now.", AuthorName = "Dev Team", AuthorEmail = "dev@iafg.net", CreatedAt = DateTime.Parse("2026-06-06T09:00:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-05T16:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-06T09:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-007", Type = "bug",
        Title = "Sidebar navigation collapses on window resize",
        Description = "When resizing the browser window, the sidebar completely collapses and cannot be reopened without refreshing the page.",
        AuthorName = "Chen, Wei", AuthorEmail = "wei.chen@iafg.net",
        Status = "New", Severity = "Low",
        Browser = "Chrome 125", Os = "macOS 14",
        Attachments = [new() { FileName = "sidebar-collapsed.png", ContentType = "image/png", Size = 28000 }],
        CreatedAt = DateTime.Parse("2026-06-04T10:30:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-04T10:30:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-008", Type = "bug",
        Title = "Session timeout doesn't redirect to login",
        Description = "After session expires (30 min idle), clicking any action shows a blank white page instead of redirecting to login.",
        AuthorName = "Martin, Sophie", AuthorEmail = "sophie.martin@iafg.net",
        Status = "New", Severity = "High",
        Browser = "Edge 124", Os = "Windows 11",
        CreatedAt = DateTime.Parse("2026-06-03T09:15:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-03T09:15:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-009", Type = "bug",
        Title = "Export to CSV produces corrupted French characters",
        Description = "CSV exports containing French characters (é, è, ê, ç) produce garbled output. The encoding seems wrong.",
        AuthorName = "Bergeron, Luc", AuthorEmail = "luc.bergeron@iafg.net",
        Status = "Resolved", Severity = "Medium",
        StepsToReproduce = "1. Export any list with French names\n2. Open CSV in Excel\n3. Characters are corrupted",
        Browser = "Chrome 125", Os = "Windows 11",
        Comments = [new() { Id = "c11", Text = "Fixed - added UTF-8 BOM to CSV export.", AuthorName = "Dev Team", AuthorEmail = "dev@iafg.net", CreatedAt = DateTime.Parse("2026-06-04T15:00:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-02T13:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-04T15:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-010", Type = "bug",
        Title = "Notification bell count doesn't update in real-time",
        Description = "The notification count badge only updates on page refresh, not when new notifications arrive via websocket.",
        AuthorName = "Tao, Dongwei", AuthorEmail = "dongwei.tao@iafg.net",
        Status = "New", Severity = "Low",
        Browser = "Chrome 125", Os = "Windows 11",
        CreatedAt = DateTime.Parse("2026-06-06T07:30:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-06T07:30:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-011", Type = "bug",
        Title = "Print preview cuts off table columns",
        Description = "When printing the contracts table, the last 2 columns (Status, Actions) are cut off. Print CSS doesn't handle wide tables.",
        AuthorName = "Lavoie, Marc", AuthorEmail = "marc.lavoie@iafg.net",
        Status = "New", Severity = "Medium",
        Attachments = [new() { FileName = "print-cutoff.png", ContentType = "image/png", Size = 52000 }],
        Browser = "Chrome 125", Os = "Windows 11",
        CreatedAt = DateTime.Parse("2026-06-05T08:45:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T08:45:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-012", Type = "bug",
        Title = "Two-factor auth code rejected intermittently",
        Description = "About 1 in 5 times, a valid 2FA code is rejected on first attempt. Entering the same code again works. Time sync issue suspected.",
        AuthorName = "Tremblay, Julie", AuthorEmail = "julie.tremblay@iafg.net",
        Status = "InProgress", Severity = "High",
        Browser = "Chrome 125", Os = "Windows 11",
        CreatedAt = DateTime.Parse("2026-06-01T11:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T14:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-013", Type = "bug",
        Title = "Dropdown menu appears behind modal overlay",
        Description = "When a modal is open and you click a dropdown inside it, the options render behind the modal backdrop and are unclickable.",
        AuthorName = "Chen, Wei", AuthorEmail = "wei.chen@iafg.net",
        Status = "Resolved", Severity = "Medium",
        Attachments = [new() { FileName = "dropdown-z-index.png", ContentType = "image/png", Size = 38000 }],
        Browser = "Safari 17", Os = "macOS 14",
        Comments = [new() { Id = "c12", Text = "Fixed z-index on dropdown portal.", AuthorName = "Dev Team", AuthorEmail = "dev@iafg.net", CreatedAt = DateTime.Parse("2026-06-03T16:00:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-02T10:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-03T16:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-014", Type = "bug",
        Title = "Auto-save indicator stuck in 'Saving...' state",
        Description = "The auto-save indicator in contract editor shows 'Saving...' permanently after a network hiccup. Data was saved but UI is stuck.",
        AuthorName = "Martin, Sophie", AuthorEmail = "sophie.martin@iafg.net",
        Status = "New", Severity = "Low",
        Browser = "Chrome 125", Os = "Windows 11",
        CreatedAt = DateTime.Parse("2026-06-06T10:15:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-06T10:15:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "bug-015", Type = "bug",
        Title = "Memory leak in Claims dashboard after extended use",
        Description = "After keeping Claims dashboard open 2+ hours, browser memory grows to 2GB+ and page becomes sluggish. Likely a polling interval not cleaned up.",
        AuthorName = "Bergeron, Luc", AuthorEmail = "luc.bergeron@iafg.net",
        Status = "InProgress", Severity = "Critical",
        Browser = "Chrome 125", Os = "Windows 11",
        Attachments = [new() { FileName = "memory-usage.png", ContentType = "image/png", Size = 41000 }],
        Comments = [new() { Id = "c13", Text = "Found it - setInterval in useEffect without cleanup. Working on fix.", AuthorName = "Dev Team", AuthorEmail = "dev@iafg.net", CreatedAt = DateTime.Parse("2026-06-06T11:00:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-05T09:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-06T11:00:00Z").ToUniversalTime()
    },

    // === FEATURE TICKETS (12 new) ===
    new()
    {
        Id = "feat-004", Type = "feature",
        Title = "Keyboard shortcuts for common actions",
        Description = "Add keyboard shortcuts for frequently used actions like creating a new ticket (Ctrl+N), searching (Ctrl+K), refreshing (F5).",
        AuthorName = "Tao, Dongwei", AuthorEmail = "dongwei.tao@iafg.net",
        Status = "New", Priority = "Nice-to-have",
        UseCase = "Power users want to navigate faster without reaching for the mouse every time.",
        CreatedAt = DateTime.Parse("2026-06-04T08:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-04T08:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-005", Type = "feature",
        Title = "Dashboard customization - drag and drop widgets",
        Description = "Allow users to customize dashboard layout by dragging and dropping widgets, resizing them, and saving preferred layout.",
        AuthorName = "Martin, Sophie", AuthorEmail = "sophie.martin@iafg.net",
        Status = "New", Priority = "Nice-to-have",
        UseCase = "Different roles need different metrics visible. One-size-fits-all dashboard doesn't work.",
        Attachments = [new() { FileName = "dashboard-mockup.png", ContentType = "image/png", Size = 67000 }],
        CreatedAt = DateTime.Parse("2026-06-03T11:30:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-03T11:30:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-006", Type = "feature",
        Title = "Audit trail for all contract modifications",
        Description = "Every change to a contract should be logged with who, when, and what changed. Viewable as a timeline.",
        AuthorName = "Lavoie, Marc", AuthorEmail = "marc.lavoie@iafg.net",
        Status = "InProgress", Priority = "Must-have",
        UseCase = "Compliance requires tracking who modified what. Currently no visibility into change history.",
        CreatedAt = DateTime.Parse("2026-05-28T09:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T10:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-007", Type = "feature",
        Title = "Multi-language support (EN/FR)",
        Description = "Application should support both English and French with a language toggle. All labels and messages should be translatable.",
        AuthorName = "Bergeron, Luc", AuthorEmail = "luc.bergeron@iafg.net",
        Status = "New", Priority = "Must-have",
        UseCase = "We operate in Quebec and must comply with Bill 96. All internal tools need French support.",
        CreatedAt = DateTime.Parse("2026-06-02T10:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-02T10:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-008", Type = "feature",
        Title = "Saved search filters and presets",
        Description = "Allow users to save frequently used search filter combinations as named presets for quick application.",
        AuthorName = "Tremblay, Julie", AuthorEmail = "julie.tremblay@iafg.net",
        Status = "New", Priority = "Nice-to-have",
        UseCase = "I run the same complex searches daily. Saving them would save 5-10 minutes per day.",
        CreatedAt = DateTime.Parse("2026-06-04T14:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-04T14:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-009", Type = "feature",
        Title = "Real-time collaboration on contract editing",
        Description = "Multiple users should edit same contract simultaneously with presence indicators, similar to Google Docs.",
        AuthorName = "Chen, Wei", AuthorEmail = "wei.chen@iafg.net",
        Status = "New", Priority = "Nice-to-have",
        UseCase = "Legal and operations often need to update contracts together. Taking turns causes delays.",
        CreatedAt = DateTime.Parse("2026-06-05T09:30:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T09:30:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-010", Type = "feature",
        Title = "Automated contract renewal reminders",
        Description = "Send automated email reminders 90, 60, and 30 days before contract expiry. Allow customizing reminder schedule per contract.",
        AuthorName = "Lavoie, Marc", AuthorEmail = "marc.lavoie@iafg.net",
        Status = "New", Priority = "Must-have",
        UseCase = "Missed several renewal deadlines this quarter. Cost $50K in penalties due to no automated reminders.",
        CreatedAt = DateTime.Parse("2026-06-01T13:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-01T13:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-011", Type = "feature",
        Title = "Role-based access control (RBAC) improvements",
        Description = "Need granular permissions: view-only for certain modules, department-level restrictions, temporary access with expiry dates.",
        AuthorName = "Tremblay, Julie", AuthorEmail = "julie.tremblay@iafg.net",
        Status = "InProgress", Priority = "Must-have",
        UseCase = "Current all-or-nothing model doesn't meet security requirements. Auditors flagged this as a risk.",
        Attachments = [new() { FileName = "rbac-proposal.png", ContentType = "image/png", Size = 55000 }],
        CreatedAt = DateTime.Parse("2026-05-25T08:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-04T16:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-012", Type = "feature",
        Title = "Integration with Microsoft Teams for notifications",
        Description = "Push important notifications (ticket updates, contract expirations, alerts) to designated Teams channels.",
        AuthorName = "Tao, Dongwei", AuthorEmail = "dongwei.tao@iafg.net",
        Status = "New", Priority = "Nice-to-have",
        UseCase = "Team lives in Teams. Having notifications there means faster response times vs checking a separate portal.",
        CreatedAt = DateTime.Parse("2026-06-03T15:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-03T15:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-013", Type = "feature",
        Title = "Advanced reporting with custom date ranges",
        Description = "Allow generating reports with custom date ranges, comparison periods, and ability to schedule recurring reports.",
        AuthorName = "Martin, Sophie", AuthorEmail = "sophie.martin@iafg.net",
        Status = "Resolved", Priority = "Must-have",
        UseCase = "Management needs reports with specific date boundaries that don't align with calendar months.",
        Comments = [new() { Id = "c14", Text = "Deployed in v2.4. Custom date picker and scheduled reports now available.", AuthorName = "Dev Team", AuthorEmail = "dev@iafg.net", CreatedAt = DateTime.Parse("2026-06-05T12:00:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-05-20T10:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T12:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-014", Type = "feature",
        Title = "Document template library",
        Description = "Create shared library of document templates (contracts, letters, reports) that users can start from.",
        AuthorName = "Bergeron, Luc", AuthorEmail = "luc.bergeron@iafg.net",
        Status = "New", Priority = "Nice-to-have",
        UseCase = "We recreate similar documents repeatedly. Templates would standardize output and save hours.",
        CreatedAt = DateTime.Parse("2026-06-04T11:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-04T11:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "feat-015", Type = "feature",
        Title = "Mobile-responsive design for field agents",
        Description = "Application should work on tablets and phones. Field agents need to access contracts and submit claims on the go.",
        AuthorName = "Chen, Wei", AuthorEmail = "wei.chen@iafg.net",
        Status = "New", Priority = "Must-have",
        UseCase = "20+ field agents use tablets during client visits. Current UI is unusable on mobile.",
        Attachments = [new() { FileName = "mobile-mockup.png", ContentType = "image/png", Size = 48000 }],
        CreatedAt = DateTime.Parse("2026-06-02T14:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-02T14:00:00Z").ToUniversalTime()
    },

    // === HELP TICKETS (12 new) ===
    new()
    {
        Id = "help-004", Type = "help",
        Title = "How to bulk import contracts from Excel?",
        Description = "We have 200 legacy contracts in Excel that need importing. Is there a bulk import tool or must we enter manually?",
        AuthorName = "Bergeron, Luc", AuthorEmail = "luc.bergeron@iafg.net",
        Status = "New", Urgency = "Non-blocking", RelatedModule = "Contracts",
        CreatedAt = DateTime.Parse("2026-06-05T10:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T10:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-005", Type = "help",
        Title = "Cannot find audit log for a deleted contract",
        Description = "Contract #CTR-2024-0892 was accidentally deleted yesterday. Need audit log to confirm who deleted it and if recovery is possible.",
        AuthorName = "Lavoie, Marc", AuthorEmail = "marc.lavoie@iafg.net",
        Status = "InProgress", Urgency = "Blocking", RelatedModule = "Contracts",
        Comments = [new() { Id = "c15", Text = "Looking into database backups now. Will update shortly.", AuthorName = "IT Support", AuthorEmail = "it.support@iafg.net", CreatedAt = DateTime.Parse("2026-06-06T09:30:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-06T09:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-06T09:30:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-006", Type = "help",
        Title = "What are the password complexity requirements?",
        Description = "New policy says we need to update passwords. What are current requirements? Length, special characters, expiry?",
        AuthorName = "Tremblay, Julie", AuthorEmail = "julie.tremblay@iafg.net",
        Status = "Resolved", Urgency = "Non-blocking", RelatedModule = "Authentication",
        Comments = [new() { Id = "c16", Text = "Min 12 chars, 1 uppercase, 1 number, 1 special char. Expires every 90 days. No reuse of last 5.", AuthorName = "IT Support", AuthorEmail = "it.support@iafg.net", CreatedAt = DateTime.Parse("2026-06-04T14:00:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-04T13:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-04T14:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-007", Type = "help",
        Title = "How to set up email forwarding for shared mailbox?",
        Description = "Team needs claims@iafg.net shared mailbox to forward to 3 members. How to configure this?",
        AuthorName = "Martin, Sophie", AuthorEmail = "sophie.martin@iafg.net",
        Status = "Resolved", Urgency = "Non-blocking", RelatedModule = "Claims",
        Comments = [new() { Id = "c17", Text = "Done! Forwarding configured. Test by sending to claims@iafg.net.", AuthorName = "IT Support", AuthorEmail = "it.support@iafg.net", CreatedAt = DateTime.Parse("2026-06-03T11:00:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-03T09:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-03T11:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-008", Type = "help",
        Title = "VPN disconnects every 30 minutes from home",
        Description = "Since last week, VPN drops every ~30 minutes requiring reconnect. Interrupts work and causes unsaved changes to be lost.",
        AuthorName = "Chen, Wei", AuthorEmail = "wei.chen@iafg.net",
        Status = "InProgress", Urgency = "Blocking", RelatedModule = "Sherlock",
        Attachments = [new() { FileName = "vpn-error-log.png", ContentType = "image/png", Size = 35000 }],
        Comments = [new() { Id = "c18", Text = "Might be related to firewall update last Friday. Escalating to network team.", AuthorName = "IT Support", AuthorEmail = "it.support@iafg.net", CreatedAt = DateTime.Parse("2026-06-06T10:00:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-06T08:30:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-06T10:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-009", Type = "help",
        Title = "Need access to Sherlock analytics module",
        Description = "Assigned to Sherlock project this week but don't have access to analytics sub-module. Manager approved it.",
        AuthorName = "Bergeron, Luc", AuthorEmail = "luc.bergeron@iafg.net",
        Status = "New", Urgency = "Blocking", RelatedModule = "Sherlock",
        CreatedAt = DateTime.Parse("2026-06-06T07:45:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-06T07:45:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-010", Type = "help",
        Title = "How to generate year-end compliance report?",
        Description = "Finance asked me to pull year-end compliance report. Can't find where it lives - under Reports or Contracts?",
        AuthorName = "Tao, Dongwei", AuthorEmail = "dongwei.tao@iafg.net",
        Status = "New", Urgency = "Non-blocking", RelatedModule = "Contracts",
        CreatedAt = DateTime.Parse("2026-06-05T16:30:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T16:30:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-011", Type = "help",
        Title = "Printer not showing in BackOffice print dialog",
        Description = "Network printer (HP LaserJet 3F) shows in Windows but not in BackOffice print dialog. Other apps see it fine.",
        AuthorName = "Tremblay, Julie", AuthorEmail = "julie.tremblay@iafg.net",
        Status = "Resolved", Urgency = "Non-blocking", RelatedModule = "One Statement",
        Comments = [new() { Id = "c19", Text = "BackOffice uses browser print dialog. Try Ctrl+P instead of in-app button.", AuthorName = "IT Support", AuthorEmail = "it.support@iafg.net", CreatedAt = DateTime.Parse("2026-06-02T15:00:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-02T14:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-02T15:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-012", Type = "help",
        Title = "Shared drive Z: not accessible from BackOffice",
        Description = "File picker doesn't show network drives when uploading. Need to upload contracts stored on Z: drive.",
        AuthorName = "Lavoie, Marc", AuthorEmail = "marc.lavoie@iafg.net",
        Status = "New", Urgency = "Non-blocking", RelatedModule = "Contracts",
        CreatedAt = DateTime.Parse("2026-06-05T11:00:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T11:00:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-013", Type = "help",
        Title = "Training request: Advanced Sherlock queries",
        Description = "Team of 5 needs training on advanced Sherlock queries. Can we schedule a session with the dev team?",
        AuthorName = "Martin, Sophie", AuthorEmail = "sophie.martin@iafg.net",
        Status = "New", Urgency = "Non-blocking", RelatedModule = "Sherlock",
        CreatedAt = DateTime.Parse("2026-06-04T09:30:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-04T09:30:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-014", Type = "help",
        Title = "Account locked after too many failed attempts",
        Description = "Mistyped password and account is locked. Need unlock ASAP - client meeting in 30 minutes.",
        AuthorName = "Bergeron, Luc", AuthorEmail = "luc.bergeron@iafg.net",
        Status = "Resolved", Urgency = "Blocking", RelatedModule = "Authentication",
        Comments = [new() { Id = "c20", Text = "Account unlocked. Consider using password manager to avoid this.", AuthorName = "IT Support", AuthorEmail = "it.support@iafg.net", CreatedAt = DateTime.Parse("2026-06-05T13:35:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-05T13:30:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-05T13:35:00Z").ToUniversalTime()
    },
    new()
    {
        Id = "help-015", Type = "help",
        Title = "How to delegate approvals while on vacation?",
        Description = "Going on vacation 2 weeks starting Monday. How to delegate contract approval authority to Sophie Martin?",
        AuthorName = "Chen, Wei", AuthorEmail = "wei.chen@iafg.net",
        Status = "InProgress", Urgency = "Non-blocking", RelatedModule = "Contracts",
        Attachments = [new() { FileName = "delegation-form.png", ContentType = "image/png", Size = 29000 }],
        Comments = [new() { Id = "c21", Text = "Set up under Settings > Delegation. Sending screenshot of the steps.", AuthorName = "IT Support", AuthorEmail = "it.support@iafg.net", CreatedAt = DateTime.Parse("2026-06-06T11:00:00Z").ToUniversalTime() }],
        CreatedAt = DateTime.Parse("2026-06-06T10:30:00Z").ToUniversalTime(),
        UpdatedAt = DateTime.Parse("2026-06-06T11:00:00Z").ToUniversalTime()
    },
};

// Delete existing test data, then insert
var ids = tickets.Select(t => t.Id).ToList();
var deleteResult = await col.DeleteManyAsync(Builders<SupportTicket>.Filter.In(t => t.Id, ids));
Console.WriteLine($"Deleted {deleteResult.DeletedCount} existing test tickets.");

await col.InsertManyAsync(tickets);
Console.WriteLine($"Inserted {tickets.Count} test tickets successfully!");

// Create placeholder screenshot images for tickets with attachments
var ticketsWithAttachments = tickets.Where(t => t.Attachments.Count > 0).ToList();
int imageCount = 0;
foreach (var ticket in ticketsWithAttachments)
{
    foreach (var att in ticket.Attachments)
    {
        CreatePlaceholderImage(ticket.Id, att.FileName, ticket.Title);
        imageCount++;
    }
}
Console.WriteLine($"Created {imageCount} placeholder screenshot images.");
Console.WriteLine();
Console.WriteLine("Summary:");
Console.WriteLine($"  - {tickets.Count(t => t.Type == "bug")} Bug tickets");
Console.WriteLine($"  - {tickets.Count(t => t.Type == "feature")} Feature tickets");
Console.WriteLine($"  - {tickets.Count(t => t.Type == "help")} Help tickets");
Console.WriteLine($"  - {imageCount} screenshot attachments");
