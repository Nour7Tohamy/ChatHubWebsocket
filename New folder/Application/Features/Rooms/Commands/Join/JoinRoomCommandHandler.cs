using Application.Features.Rooms.Commands.Join;
using Application.Infrastructure.Repositories;
using Domain.Entities.Main;
using MediatR;

namespace Application.Features.Rooms.Commands.JoinRoom;

public class JoinRoomCommandHandler : IRequestHandler<JoinRoomCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public JoinRoomCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        var isMember = await _unitOfWork.Rooms.IsUserMemberAsync(request.RoomId, request.UserId);

        if (isMember)
            throw new Exception("User is already a member of this room");

        var member = new RoomMember
        {
            RoomId = request.RoomId,
            UserId = request.UserId
        };

        await _unitOfWork.Rooms.AddMemberAsync(request.RoomId, request.UserId);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}