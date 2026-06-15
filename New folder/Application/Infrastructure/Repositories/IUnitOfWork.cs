using Application.Infrastructure.Repositories;

namespace Application.Infrastructure.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRoomRepository Rooms { get; }
    IMessageRepository Messages { get; }
    Task<int> SaveChangesAsync();
}