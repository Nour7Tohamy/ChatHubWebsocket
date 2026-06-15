using Domain.Entities.Main;

namespace Application.Infrastructure.Repositories;

public interface IRoomRepository : IGenericRepository<Room>
{
    Task<Room?> GetRoomWithMembersAsync(Guid roomId);
    Task<bool> IsUserMemberAsync(Guid roomId, string userId);
    Task AddMemberAsync(Guid roomId, string userId);
    Task RemoveMemberAsync(Guid roomId, string userId);
    Task<IEnumerable<Room>> GetAllWithMembersAsync(); // ✅ جديدة
}