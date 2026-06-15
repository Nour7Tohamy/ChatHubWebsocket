using Application.Features.Rooms.Commands.Delete;
using Application.Infrastructure.Repositories;
using MediatR;

namespace Application.Features.Rooms.Commands.DeleteRoom;

public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoomCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId)
            ?? throw new Exception("Room not found");

        if (room.OwnerId != request.UserId)
            throw new Exception("Only the owner can delete this room");

        _unitOfWork.Rooms.Delete(room);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}