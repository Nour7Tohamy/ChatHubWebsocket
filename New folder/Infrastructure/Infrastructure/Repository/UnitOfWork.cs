using Application.Infrastructure.Repositories;
using Infrastructure.Data;
using Infrastructure.Infrastructure.Repository;

namespace Infrastructure.Infrastructure.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IRoomRepository Rooms { get; }
    public IMessageRepository Messages { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Rooms = new RoomRepository(context);
        Messages = new MessageRepository(context);
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public void Dispose() =>
        _context.Dispose();
}