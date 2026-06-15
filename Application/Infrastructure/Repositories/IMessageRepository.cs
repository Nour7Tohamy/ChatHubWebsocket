using Domain.Entities.Main;

namespace Application.Infrastructure.Repositories
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<IEnumerable<Message>> GetRoomMessagesAsync(Guid roomId, int page, int pageSize);
        Task<IEnumerable<Message>> GetPrivateMessagesAsync(string senderId, string receiverId);
    }
}
