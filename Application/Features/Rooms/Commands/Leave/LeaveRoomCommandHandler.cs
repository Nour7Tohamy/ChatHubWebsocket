using Application.Features.Rooms.Commands.Leave;
using Application.Infrastructure.Repositories;
using MediatR;

namespace Application.Features.Rooms.Commands.LeaveRoom;

public class LeaveRoomCommandHandler : IRequestHandler<LeaveRoomCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public LeaveRoomCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(LeaveRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId)
            ?? throw new Exception("Room not found");

        if (room.OwnerId == request.UserId)
            throw new Exception("Owner cannot leave the room");

        await _unitOfWork.Rooms.RemoveMemberAsync(request.RoomId, request.UserId);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}