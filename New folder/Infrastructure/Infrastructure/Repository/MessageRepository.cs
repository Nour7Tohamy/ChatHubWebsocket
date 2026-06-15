using Application.Infrastructure.Repositories;
using Infrastructure.Data;
using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Infrastructure.Repository;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(ApplicationDbContext context) : base(context) { }

    // Get messages for a specific room with pagination
    public async Task<IEnumerable<Message>> GetRoomMessagesAsync(Guid roomId, int page, int pageSize) =>
        await _context.Messages
            .Where(m => m.RoomId == roomId)
            .Include(m => m.Sender)
            .OrderBy(m => m.SentAt)        // ✅ أقدم رسالة فوق
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    // Get private messages between two users
    public async Task<IEnumerable<Message>> GetPrivateMessagesAsync(string senderId, string receiverId) =>
        await _context.Messages
            .Where(m =>
                (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                (m.SenderId == receiverId && m.ReceiverId == senderId))
            .Include(m => m.Sender)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
}