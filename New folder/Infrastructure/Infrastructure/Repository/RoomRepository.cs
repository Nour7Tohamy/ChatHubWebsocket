using Application.Infrastructure.Repositories;
using Infrastructure.Data;
using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Infrastructure.Repository;

public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context) { }
    public async Task<Room?> GetRoomWithMembersAsync(Guid roomId) =>
      await _context.Rooms
          .Include(r => r.Members)
          .Include(r => r.Owner)
          .FirstOrDefaultAsync(r => r.Id == roomId);

    public async Task<IEnumerable<Room>> GetAllWithMembersAsync() =>
    await _context.Rooms
        .Include(r => r.Members)
        .Include(r => r.Owner)
        .Where(r => !r.IsDeleted)
        .ToListAsync();

    public async Task<bool> IsUserMemberAsync(Guid roomId, string userId) =>
        await _context.RoomMembers
            .AnyAsync(rm => rm.RoomId == roomId && rm.UserId == userId);

    public async Task AddMemberAsync(Guid roomId, string userId)
    {
        var membership = new RoomMember
        {
            RoomId = roomId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };
        _context.RoomMembers.Add(membership);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid roomId, string userId)
    {
        var membership = await _context.RoomMembers
            .FirstOrDefaultAsync(rm => rm.RoomId == roomId && rm.UserId == userId);
        if (membership != null)
        {
            _context.RoomMembers.Remove(membership);
            await _context.SaveChangesAsync();
        }
    }
}