using Microsoft.EntityFrameworkCore;
using recrutementapp.Data;
using recrutementapp.Models;

namespace recrutementapp.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;
    public NotificationService(ApplicationDbContext db) => _db = db;

    public async Task CreateAsync(int userId, string type, string content, string? link = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId  = userId,
            Type    = type,
            Content = content,
            Link    = link
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetUnreadAsync(int userId)
        => await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .ToListAsync();

    public async Task MarkAllReadAsync(int userId)
    {
        var list = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        foreach (var n in list) n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
}
